﻿using System.Collections.Generic;
using UnityEngine;
using KS.Reactor;

namespace KS.SceneFusion2.Unity.Editor
{
    class sfTreePrototypeSync
    {
        private delegate void PrototypeSetter(TreePrototype prototype, sfBaseProperty property);
        private Dictionary<string, PrototypeSetter> m_setters = new Dictionary<string, PrototypeSetter>();

        public sfTreePrototypeSync()
        {
            RegisterPrototypeSetters();
        }

        private void RegisterPrototypeSetters()
        {
            m_setters[sfProp.Prefab] = (TreePrototype prototype, sfBaseProperty property) =>
            {
                prototype.prefab = sfPropertyUtils.ToReference(property, prototype.prefab);
            };
            m_setters[sfProp.BendFactor] = (TreePrototype prototype, sfBaseProperty property) =>
            {
                prototype.bendFactor = (float)property;
            };
        }

        /**
         * Serializes a tree prototype.
         * 
         * @param   TreePrototype prototype to serialize.
         * @param   ref bool updated - set to true if a stand-in asset on the prototype was replaced with the correct
         *          asset.
         * @return  sfDictionaryProperty the tree prototype as a dictionary property.
         */
        public sfDictionaryProperty Serialize(TreePrototype prototype, ref bool updated)
        {
            sfDictionaryProperty dict = new sfDictionaryProperty();
            dict[sfProp.Prefab] = sfPropertyUtils.FromReference(prototype.prefab);
            GameObject newPrefab = sfLoader.Get().GetAssetForStandIn(prototype.prefab);
            if (newPrefab != null)
            {
                prototype.prefab = newPrefab;
                updated = true;
            }
            dict[sfProp.BendFactor] = new sfValueProperty(prototype.bendFactor);
            return dict;
        }

        public TreePrototype Deserialize(sfDictionaryProperty dict)
        {
            TreePrototype prototype = new TreePrototype();
            UpdatePrototype(prototype, dict);
            return prototype;
        }

        /**
         * Update the prototype fields from a property
         */
        public void UpdatePrototype(TreePrototype prototype, sfBaseProperty property)
        {
            PrototypeSetter setter;
            if (property.Type == sfBaseProperty.Types.DICTIONARY)
            {
                sfDictionaryProperty dict = property as sfDictionaryProperty;
                foreach (KeyValuePair<string, sfBaseProperty> pair in dict)
                {
                    if (m_setters.TryGetValue(pair.Value.Name, out setter))
                    {
                        setter(prototype, pair.Value);
                    }
                }
            }
            else if (m_setters.TryGetValue(property.Name, out setter))
            {
                setter(prototype, property);
            }
        }

        /**
         * Update properties from a prototype
         * 
         * @return  bool true if a stand-on the prototype was replaced with the correct asset.
         **/
        public bool UpdateProperties(sfDictionaryProperty dict, TreePrototype prototype)
        {
            bool updated = false;
            sfPropertyManager.Get().Copy(dict[sfProp.Prefab], sfPropertyUtils.FromReference(prototype.prefab));
            GameObject newPrefab = sfLoader.Get().GetAssetForStandIn(prototype.prefab);
            if (newPrefab != null)
            {
                prototype.prefab = newPrefab;
                updated = true;
            }
            UpdateProperty(dict[sfProp.BendFactor], prototype.bendFactor);
            return updated;
        }

        public void UpdateProperty(sfBaseProperty property, ksMultiType value)
        {
            if (property.Type == sfBaseProperty.Types.VALUE)
            {
                if (!(property as sfValueProperty).Value.Equals(value))
                {
                    (property as sfValueProperty).Value = value;
                }
                return;
            }

            if (property.Type == sfBaseProperty.Types.STRING)
            {
                if ((property as sfStringProperty).String != value.String)
                {
                    (property as sfStringProperty).String = value.String;
                }
                return;
            }
        }
    }
}
