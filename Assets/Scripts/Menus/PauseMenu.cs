using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Globals;

public class PauseMenu : Menu
{
    public void PauseGame()
    {
        thisMenu.enabled = true;
        gamePaused = true;
    }

    public void OnToggleHints()
    {
        ToggleHints();
    }

    public void OnToggleSound()
    {
        ToggleSound();
    }

    public void OnSelectResume()
    {
        StartCoroutine(ResumeGame());
    }

    //this is a coroutine because otherwise, clicking the resume button would register a click on the game board on the same frame
    IEnumerator ResumeGame()
    {
        thisMenu.enabled = false;
        yield return new WaitForEndOfFrame();
        gamePaused = false;
    }

    public void OnSelectBackToMainMenu()
    {
        UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(0);
    }
}