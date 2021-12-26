using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static Globals;

public class SettingsMenu : Menu
{
    [SerializeField] UserSettings userSettings;

    [Space]

    [SerializeField] Toggle hintsToggle;
    [SerializeField] Toggle soundToggle;

    [SerializeField] TMP_Dropdown cpuDifficultyDropdown;

    [Space]

    [SerializeField] RectTransform backgroundImages;
    Toggle[] backgroundToggles;
    public event System.Action BackgroundChangeAction;

    protected override void Awake()
    {
        base.Awake();
        InitOptions();
    }

    //init. options based on UserSettings values
    void InitOptions()
    {
        backgroundToggles = backgroundImages.GetComponentsInChildren<Toggle>();

        hintsToggle.isOn = userSettings.hintsOn;
        soundToggle.isOn = userSettings.soundOn;

        cpuDifficultyDropdown.value = (int)userSettings.cpuDifficulty;

        for (int i = 0; i < backgroundToggles.Length; i++)
        {
            backgroundToggles[i].SetIsOnWithoutNotify(i == (int)userSettings.backgroundImage);
        }
    }

    public void OnToggleHints()
    {
        userSettings.hintsOn = hintsToggle.isOn;
    }

    public void OnToggleSound()
    {
        userSettings.soundOn = soundToggle.isOn;
    }

    public void OnChangeCPUDifficulty()
    {
        userSettings.cpuDifficulty = (UserSettings.CPUDifficulty)cpuDifficultyDropdown.value;
    }

    public void OnChangeBackgroundImage(int value)
    {
        if (backgroundToggles[value].isOn)
        {
            userSettings.backgroundImage = (UserSettings.BackgroundImage)value;
            BackgroundChangeAction?.Invoke();
        }
    }

    public void SaveSettings()
    {
        userSettings.Save();
    }

    #region Game scene functions

    public void ShowOptionsMenu()
    {
        Open();
        inputEnabled = false;
    }

    public void OnSelectResume()
    {
        StartCoroutine(HideOptionsMenu());
    }

    //this is a coroutine because otherwise, clicking the resume button would register a click on the game board on the same frame
    IEnumerator HideOptionsMenu()
    {
        Close();
        yield return null;
        inputEnabled = true;
    }

    #endregion
}