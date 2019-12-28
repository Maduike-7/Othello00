using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static Globals;

public class OptionsMenu : Menu
{
    [SerializeField] Toggle hintsToggle, soundToggle;
    [SerializeField] TMP_Dropdown cpuDifficultyDropdown;

    protected override void Awake()
    {
        base.Awake();
        InitOptions();
    }

    //init. options based on PlayerPrefs
    void InitOptions()
    {
        hintsToggle.isOn = PlayerPrefs.GetInt("Hints", 1) == 1 ? true : false;
        hintsEnabled = hintsToggle.isOn;

        soundToggle.isOn = PlayerPrefs.GetInt("Sound", 1) == 1 ? true : false;
        soundEnabled = soundToggle.isOn;

        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex == 0)
        {
            cpuDifficultyDropdown.value = PlayerPrefs.GetInt("CPU difficulty", 0);
            cpuDifficulty = cpuDifficultyDropdown.value;
        }
    }

    public void OnToggleHints()
    {
        ToggleHints();
    }

    public void OnToggleSound()
    {
        ToggleSound();
    }

    public void OnCPUDifficultyChanged()
    {
        ChangeCPUDifficulty(cpuDifficultyDropdown.value);
    }

    #region Game scene functions

    public void ShowOptionsMenu()
    {
        OpenMenu(thisMenu);
        inputEnabled = false;
    }

    public void OnSelectResume()
    {
        StartCoroutine(HideOptionsMenu());
    }

    //this is a coroutine because otherwise, clicking the resume button would register a click on the game board on the same frame
    IEnumerator HideOptionsMenu()
    {
        CloseMenu(thisMenu);
        yield return new WaitForEndOfFrame();
        inputEnabled = true;
    }
    #endregion
}