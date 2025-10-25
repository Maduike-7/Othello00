using UnityEngine;
using UnityEngine.UI;

public class GameSettingsMenu : Menu
{
    [SerializeField] UserSettings userSettings;

    [Space]

    [SerializeField] Toggle clockToggle;
    [SerializeField] Toggle soundToggle;
    [SerializeField] Toggle hintsToggle;
    [SerializeField] Toggle animationsToggle;

    public event System.Action ToggleClockAction;

    protected override void Awake()
    {
        InitOptions();
    }

    void InitOptions()
    {
        clockToggle.isOn = userSettings.clockOn;
        soundToggle.isOn = userSettings.soundOn;
        hintsToggle.isOn = userSettings.hintsOn;
        animationsToggle.isOn = userSettings.animationsOn;
    }

    public void OnToggleHints()
    {
        userSettings.hintsOn = hintsToggle.isOn;
    }

    public void OnToggleSound()
    {
        userSettings.soundOn = soundToggle.isOn;
    }

    public void OnToggleAnimations()
    {
        userSettings.animationsOn = animationsToggle.isOn;
    }

    public void OnToggleClock()
    {
        userSettings.clockOn = clockToggle.isOn;
        ToggleClockAction?.Invoke();
    }

    public void HandleBackButtonInput()
    {
        userSettings.Save();
    }
}