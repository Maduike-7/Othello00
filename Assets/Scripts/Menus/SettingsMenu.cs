using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static Globals;

public class SettingsMenu : Menu
{
    [SerializeField] Toggle hintsToggle;
    [SerializeField] Toggle soundToggle;
    [SerializeField] TMP_Dropdown cpuDifficultyDropdown;

    protected override void Awake()
    {
        base.Awake();
        LoadSettings();
    }

    //init. settings based on PlayerPrefs
    void LoadSettings()
    {
        int ppHintsValue = PlayerPrefs.GetInt("Hints", 1);
        hintsToggle.isOn = ppHintsValue == 1 ? true : false;
        hintsEnabled = hintsToggle.isOn;

        int ppSoundValue = PlayerPrefs.GetInt("Sound", 1);
        soundToggle.isOn = ppSoundValue == 1 ? true : false;
        soundEnabled = soundToggle.isOn;

        int ppCpuDifficulty = PlayerPrefs.GetInt("CPU difficulty", 0);
        cpuDifficultyDropdown.value = ppCpuDifficulty;
        cpuDifficulty = (CPUDifficulty)cpuDifficultyDropdown.value;
    }

    public void OnHintsToggled()
    {
        ToggleHints();
    }

    public void OnSoundToggled()
    {
        ToggleSound();
    }

    public void OnCPUDifficultyChanged()
    {
        ChangeCPUDifficulty(cpuDifficultyDropdown.value);
    }
}