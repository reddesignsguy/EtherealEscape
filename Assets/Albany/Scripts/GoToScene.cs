using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GoToScene : MonoBehaviour
{
    
    /// <summary>
    /// Given the name of a scene, loads that scene.
    /// 
    /// The scene must be in the "Scenes in Build" list
    /// in the Build Manager for this to work.
    /// </summary>
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

}
