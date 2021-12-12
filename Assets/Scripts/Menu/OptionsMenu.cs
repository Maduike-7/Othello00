using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static Globals;

public class OptionsMenu : Menu
{
    [SerializeField] UserSettings userSettings;

    [Space]

    [SerializeField] Toggle hintsToggle;
    [SerializeField] Toggle soundToggle;

    [SerializeField] TMP_Dropdown cpuDifficultyDropdown;

    [Space]

    [SerializeField] RectTransform backgroundImages;
    public event System.Action BackgroundChangeAction;

    protected override void Awake()
    {
        base.Awake();
        InitOptions();
    }

    //init. options based on UserSettings values
    void InitOptions()
    {
        hintsToggle.isOn = userSettings.hintsOn;
        soundToggle.isOn = userSettings.soundOn;

        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex == 0)
        {
            cpuDifficultyDropdown.value = (int)userSettings.cpuDifficulty;
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
        if (backgroundImages.GetComponentsInChildren<Toggle>()[value].isOn)
        {
            userSettings.backgroundImage = (UserSettings.BackgroundImage)value;
            BackgroundChangeAction?.Invoke();
        }
    }

    public void SaveSettings()
    {
        FileHandler.SaveSettings(userSettings);
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