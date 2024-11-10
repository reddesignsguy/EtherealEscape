﻿using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEditor;
using KS.SceneFusion2.Client;
using KS.SceneFusion;
using KS.Reactor;
using KS.Unity.Editor;

using UObject = UnityEngine.Object;

namespace KS.SceneFusion2.Unity.Editor
{
    /**
     * Utility class for converting between sfProperties and common types.
     */
    public class sfPropertyUtils
    {
        private static readonly string LOG_CHANNEL = typeof(sfPropertyUtils).ToString();

#if !UNITY_2022_1_OR_NEWER
        private static ksReflectionObject m_roGradient;

        /**
         * Static constructor
         */
        static sfPropertyUtils()
        {
            m_roGradient = new ksReflectionObject(typeof(SerializedProperty)).GetProperty("gradientValue");
        }
#endif

        /**
         * Gets a gradient value from a serialized property.
         * 
         * @param   SerializedProperty sprop to get gradient from.
         * @return  Gradient value from the property.
         */
        public static Gradient GetGradient(SerializedProperty sprop)
        {
#if UNITY_2022_1_OR_NEWER
            return sprop.gradientValue;
#else
            return (Gradient)m_roGradient.GetValue(sprop);
#endif
        }

        /**
         * Sets a gradient value on a serialized property.
         * 
         * @param   SerializedProperty sprop to set gradient on.
         * @param   Gradient gradient value to set.
         */
        public static void SetGradient(SerializedProperty sprop, Gradient value)
        {
#if UNITY_2022_1_OR_NEWER
            sprop.gradientValue = value;
#else
            m_roGradient.SetValue(sprop, value);
#endif
        }

        /**
         * Applies serialized property changes without registering an undo operation, and marks the target object dirty
         * if there were any changes.
         * 
         * @param   SerializedObject so - serialized object to apply properties to.
         * @param   bool rebuildInspectors - if true and properties changed, rebuilds inspectors, which is slower but
         *          necessary for it to display properly if components were added or deleted.
         * @return  bool true if there were any changes applied.
         */
        public static bool ApplyProperties(SerializedObject so, bool rebuildInspectors = false)
        {
            if (so.ApplyModifiedPropertiesWithoutUndo())
            {
                EditorUtility.SetDirty(so.targetObject);
                sfUI.Get().MarkInspectorStale(so.targetObject, rebuildInspectors);
                return true;
            }
            return false;
        }

        /**
         * Converts a uobject reference to an sfReferenceProperty or sfNullProperty. Syncs the uobject if it can be
         * synced, otherwise if it is an asset, creates an asset path sfObject for it if one does not already exist.
         * 
         * @param   UObject uobj to convert.
         * @param   SerializedProperty sprop referencing the uobject. If the uobject is a stand-in for an asset that is
         *          available, updates the reference on this property to the correct asset.
         */
        public static sfBaseProperty FromReference(UObject uobj, SerializedProperty sprop = null)
        {
            if (uobj == null)
            {
                return new sfNullProperty();
            }

            sfObject obj = sfObjectMap.Get().GetSFObject(uobj);
            if (obj == null)
            {
                // Create an sfObject for the uobject if it can be synced.
                if (!sfLoader.Get().IsAsset(uobj) || sfLoader.Get().IsSyncableAssetType(uobj))
                {
                    obj = sfObjectEventDispatcher.Get().Create(uobj);
                }
                if (obj == null && sfLoader.Get().IsAsset(uobj))
                {
                    sfAssetInfo assetInfo = sfLoader.Get().GetAssetInfo(uobj);

                    // Get the asset path sfObject for the asset, or create one if it does not exist.
                    sfAssetPathTranslator translator = sfObjectEventDispatcher.Get()
                        .GetTranslator<sfAssetPathTranslator>(sfType.AssetPath);
                    obj = translator.GetOrCreatePathObject(assetInfo, uobj);

                    // If the uobject is a stand-in for an asset that is available, replace it with the correct asset.
                    if (sprop != null && sfLoader.Get().IsStandIn(uobj))
                    {
                        UObject asset = sfLoader.Get().GetAssetForStandIn(uobj);
                        if (asset != null)
                        {
                            if (sprop.propertyType == SerializedPropertyType.ObjectReference)
                            {
                                sprop.objectReferenceValue = asset;
                            }
                            else if (sprop.propertyType == SerializedPropertyType.ExposedReference)
                            {
                                sprop.exposedReferenceValue = asset;
                            }
                            sprop.serializedObject.ApplyModifiedPropertiesWithoutUndo();
                        }
                    }
                }
                if (obj == null)
                {
                    ksLog.Warning(LOG_CHANNEL, "Unable to sync reference to " + uobj.GetType().Name + " " + uobj.name);
                    // 0 means keep your current value; when we cannot sync a reference to something, we don't want to
                    // overwrite the reference for other users in case they are already referencing the correct object.
                    return new sfReferenceProperty(0);
                }
            }
            return new sfReferenceProperty(obj.Id);
        }

        /**
         * Converts a property to a T reference. T must be a UObject.
         * 
         * @param   sfBaseProperty prop to convert.
         * @param   T current reference value. This is returned if the property cannot be converted.
         */
        public static T ToReference<T>(sfBaseProperty prop, T current = null) where T : UObject
        {
            if (prop.Type == sfBaseProperty.Types.NULL)
            {
                return null;
            }
            if (prop.Type != sfBaseProperty.Types.REFERENCE)
            {
                return current;
            }
            uint objId = ((sfReferenceProperty)prop).ObjectId;
            if (objId == 0)
            {
                // 0 means keep the current value.
                return current;
            }
            sfObject obj = SceneFusion.Get().Service.Session.GetObject(objId);
            if (obj == null)
            {
                return current;
            }
            return sfObjectEventDispatcher.Get().GetUObject(obj, current) as T;
        }

        /// <summary>
        /// Gets an <see cref="sfAssetInfo"/> from path, class, and index fields in a dictionary property.
        /// </summary>
        /// <param name="dict">
        /// Dictionary property with path, (optionally) class, and (optionally) index fields.
        /// </param>
        /// <param name="type">
        /// If provided, the asset info will use this type for the class name instead of looking for a class field in
        /// the dictionary.
        /// </param>
        /// <returns>Asset info</returns>
        public static sfAssetInfo GetAssetInfo(sfDictionaryProperty dict, Type type = null)
        {
            sfAssetInfo info = new sfAssetInfo();
            info.ClassName = type == null ? (string)dict[sfProp.Class] : type.ToString();
            info.Path = (string)dict[sfProp.Path];
            sfBaseProperty indexProp;
            if (dict.TryGetField(sfProp.Index, out indexProp))
            {
                info.Index = (int)indexProp;
            }
            return info;
        }

        /// <summary>
        /// Sets path and optionally class fields from an asset info in a dictionary property. If the asset info is for
        /// a subasset, also sets the index field.
        /// </summary>
        /// <param name="dict">Dictionary property to set fields in.</param>
        /// <param name="info">Asset info to get field values from.</param>
        /// <param name="includeClassName">
        /// If true, sets the class field to <see cref="sfAssetInfo.ClassName"/>.
        /// </param>
        public static void SetAssetInfoProperties(
            sfDictionaryProperty dict,
            sfAssetInfo info,
            bool includeClassName = true)
        {
            if (!info.IsValid)
            {
                throw new ArgumentException("Invalid asset info.");
            }
            if (includeClassName)
            {
                dict[sfProp.Class] = info.ClassName;
            }
            dict[sfProp.Path] = info.Path;
            if (info.IsSubasset)
            {
                dict[sfProp.Index] = info.Index;
            }
        }

        /**
         * Converts an AnimationCurve to an sfValueProperty.
         * 
         * @param   AnimationCurve value to convert.
         * @return  sfValueProperty the AnimationCurve as a property.
         */
        public static sfValueProperty FromAnimationCurve(AnimationCurve value)
        {
            byte[] data = new byte[Marshal.SizeOf(typeof(Keyframe)) * value.keys.Length + 1];
            int offset = 0;
            foreach (Keyframe frame in value.keys)
            {
                offset += Reactor.ksFixedDataWriter.WriteData(data, data.Length, offset, frame);
            }
            data[offset] = (byte)((byte)value.preWrapMode | ((byte)value.postWrapMode << 3));
            return new sfValueProperty(data);
        }

        /**
         * Converts an sfProperty to an AnimationCurve.
         * 
         * @param   sfBaseProperty property to convert.
         * @return  AnimationCurve value of the property.
         *          Null if the property could not be converted to an AnimationCurve.
         */
        public static AnimationCurve ToAnimationCurve(sfBaseProperty property)
        {
            if (property == null || property.Type != sfBaseProperty.Types.VALUE)
            {
                return null;
            }

            try
            {
                byte[] data = ((sfValueProperty)property).Value.ByteArray;
                int offset = 0;
                int sizeOfKeyframe = Marshal.SizeOf(typeof(Keyframe));
                int length = (data.Length - 1) / sizeOfKeyframe;
                Keyframe[] frames = new Keyframe[length];
                for (int i = 0; i < length; i++)
                {
                    frames[i] = ksFixedDataParser.ParseFromBytes<Keyframe>(data, offset);
                    offset += sizeOfKeyframe;
                }
                AnimationCurve animationCurve = new AnimationCurve(frames);
                byte preWrapMode = (byte)(data[offset] & 7);
                byte postWrapMode = (byte)((data[offset] >> 3) & 7);
                animationCurve.preWrapMode = (WrapMode)preWrapMode;
                animationCurve.postWrapMode = (WrapMode)postWrapMode;
                return animationCurve;
            }
            catch (Exception e)
            {
                ksLog.Error(
                    LOG_CHANNEL,
                    "Error syncing AnimationCurve. Your script source code may be out of sync.", e);
                return new AnimationCurve();
            }
        }

        /**
         * Converts a Gradient to an sfValueProperty.
         * 
         * @param   Gradient value to convert.
         * @return  sfValueProperty the Gradient as a property.
         */
        public static sfValueProperty FromGradient(Gradient value)
        {
            byte[] data = new byte[sizeof(int) +
                Marshal.SizeOf(typeof(GradientColorKey)) * value.colorKeys.Length +
                Marshal.SizeOf(typeof(GradientAlphaKey)) * value.alphaKeys.Length + 1];
            int offset = 0;
            offset += ksFixedDataWriter.WriteData(data, data.Length, offset, value.colorKeys.Length);
            foreach (GradientColorKey colourKey in value.colorKeys)
            {
                offset += ksFixedDataWriter.WriteData(data, data.Length, offset, colourKey);
            }
            foreach (GradientAlphaKey alphaKey in value.alphaKeys)
            {
                offset += ksFixedDataWriter.WriteData(data, data.Length, offset, alphaKey);
            }
            offset += ksFixedDataWriter.WriteData(data, data.Length, offset, (byte)value.mode);
            return new sfValueProperty(data);
        }

        /**
         * Converts an sfProperty to a Gradient.
         * 
         * @param   sfBaseProperty property to convert.
         * @return  Gradient value of the property.
         *          Null if the property could not be converted to an Gradient.
         */
        public static Gradient ToGradient(sfBaseProperty property)
        {
            if (property == null || property.Type != sfBaseProperty.Types.VALUE)
            {
                return null;
            }

            try
            {
                int sizeOfColorKey = Marshal.SizeOf(typeof(GradientColorKey));
                int sizeOfAlphaKey = Marshal.SizeOf(typeof(GradientAlphaKey));
                byte[] data = ((sfValueProperty)property).Value.ByteArray;
                Gradient gradient = new Gradient();
                int offset = 0;
                int colorKeyNum = ksFixedDataParser.ParseFromBytes<int>(data, offset);
                GradientColorKey[] colourKeys = new GradientColorKey[colorKeyNum];
                offset += sizeof(int);
                int alphaKeyNum = (data.Length - sizeof(int) - colorKeyNum * sizeOfColorKey - 1) / sizeOfAlphaKey;
                GradientAlphaKey[] alphaKeys = new GradientAlphaKey[alphaKeyNum];
                for (int i = 0; i < colourKeys.Length; i++)
                {
                    colourKeys[i] = ksFixedDataParser.ParseFromBytes<GradientColorKey>(data, offset);
                    offset += sizeOfColorKey;
                }
                for (int i = 0; i < alphaKeys.Length; i++)
                {
                    alphaKeys[i] = ksFixedDataParser.ParseFromBytes<GradientAlphaKey>(data, offset);
                    offset += sizeOfAlphaKey;
                }
                gradient.colorKeys = colourKeys;
                gradient.alphaKeys = alphaKeys;
                gradient.mode = (GradientMode)data[offset];
                return gradient;
            }
            catch (Exception e)
            {
                ksLog.Error(LOG_CHANNEL, "Error syncing Gradient. Your script source code may be out of sync.", e);
                return new Gradient();
            }
        }
    }
}
