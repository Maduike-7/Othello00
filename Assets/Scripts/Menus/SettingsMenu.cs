using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static Globals;

public class SettingsMenu : Menu
{
    [SerializeField] Toggle hintsToggle, soundToggle;
    [SerializeField] TMP_Dropdown cpuDifficultyDropdown;

    protected override void Awake()
    {
        base.Awake();
        LoadSettings();
    }

    //init. settings based on PlayerPrefs
    void LoadSettings()
    {
        hintsToggle.isOn = PlayerPrefs.GetInt("Hints", 1) == 1 ? true : false;
        hintsEnabled = hintsToggle.isOn;
        
        soundToggle.isOn = PlayerPrefs.GetInt("Sound", 1) == 1 ? true : false;
        soundEnabled = soundToggle.isOn;
        
        cpuDifficultyDropdown.value = PlayerPrefs.GetInt("CPU difficulty", 0);
        cpuDifficulty = cpuDifficultyDropdown.value;
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
}