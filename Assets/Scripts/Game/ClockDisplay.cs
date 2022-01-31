using System.Collections;
using UnityEngine;
using TMPro;

public class ClockDisplay : MonoBehaviour
{
    [SerializeField] UserSettings userSettings;
    [SerializeField] GameSettingsMenu gameSettingsMenu;

    const string TimeFormat = "mm':'ss";
    TextMeshProUGUI timeText;

    float currentTime;

    void Awake()
    {
        timeText = GetComponent<TextMeshProUGUI>();

        gameSettingsMenu.ToggleClockAction += OnClockToggled;
        FindObjectOfType<PauseHandler>().GamePauseAction += OnGamePaused;
    }

    void Start()
    {
        OnClockToggled();
    }

    void Update()
    {
        currentTime += Time.deltaTime;
        timeText.text = System.TimeSpan.FromSeconds(currentTime).ToString(TimeFormat);
    }

    void OnClockToggled()
    {
        timeText.enabled = userSettings.clockOn;
    }

    void OnGamePaused(bool state)
    {
        enabled = !state;
    }
}