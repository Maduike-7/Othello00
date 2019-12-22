using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : Menu
{
    public void OnSelectPlay()
    {
        UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(1);
    }

    public void OnSelectInstructions(Canvas instructionsMenu)
    {
        instructionsMenu.enabled = true;
        thisMenu.enabled = false;
    }

    public void OnSelectOptions(Canvas optionsMenu)
    {
        optionsMenu.enabled = true;
        thisMenu.enabled = false;
    }

    public void OnSelectQuit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else 
        Application.Quit();
#endif
    }
}