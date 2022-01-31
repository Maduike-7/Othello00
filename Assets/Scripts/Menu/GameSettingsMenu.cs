using UnityEngine;
using UnityEngine.UI;

public class GameSettingsMenu : Menu
{
    [SerializeField] UserSettings userSettings;

    [Space]

    [SerializeField] Toggle soundToggle;
    [SerializeField] Toggle hintsToggle;
    [SerializeField] Toggle animationsToggle;
    [SerializeField] Toggle clockToggle;

    protected override void Awake()
    {
        InitOptions();
    }

    void InitOptions()
    {
        hintsToggle.isOn = userSettings.hintsOn;
        soundToggle.isOn = userSettings.soundOn;
        animationsToggle.isOn = userSettings.animationsOn;
        clockToggle.isOn = userSettings.clockOn;
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
    }

    public override void HandleBackButtonInput()
    {
        userSettings.Save();
    }
}