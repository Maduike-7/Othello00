using UnityEngine;
using UnityEngine.UI;
using TMPro;

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

    [SerializeField] RectTransform boardColours;
    Toggle[] colourToggles;

    protected override void Awake()
    {
        base.Awake();
        InitOptions();
    }

    //init. options based on UserSettings values
    void InitOptions()
    {
        backgroundToggles = backgroundImages.GetComponentsInChildren<Toggle>();
        colourToggles = boardColours.GetComponentsInChildren<Toggle>();

        hintsToggle.isOn = userSettings.hintsOn;
        soundToggle.isOn = userSettings.soundOn;

        cpuDifficultyDropdown.value = (int)userSettings.cpuDifficulty;

        for (int i = 0; i < backgroundToggles.Length; i++)
        {
            backgroundToggles[i].SetIsOnWithoutNotify(i == (int)userSettings.backgroundImage);
        }

        for (int i = 0; i < colourToggles.Length; i++)
        {
            colourToggles[i].SetIsOnWithoutNotify(i == (int)userSettings.boardColour);
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

    public void OnChangeBoardColour(int value)
    {
        if (colourToggles[value].isOn)
        {
            userSettings.boardColour = (UserSettings.BoardColour)value;
        }
    }

    public void SaveSettings()
    {
        userSettings.Save();
    }
}