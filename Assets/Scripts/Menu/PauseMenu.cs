using UnityEngine;

public class PauseMenu : Menu
{
    [SerializeField] PauseHandler pauseHandler;

    [Space]

    [SerializeField] GameObject pauseMenuPanel;
    [SerializeField] GameObject settingsMenuPanel;

    protected override void Awake()
    {
        base.Awake();
        pauseHandler.GamePauseAction += OnGamePaused;
    }

    void OnGamePaused(bool state)
    {
        if (state)
        {
            Open();
        }
        else
        {
            pauseMenuPanel.SetActive(true);
            settingsMenuPanel.SetActive(false);

            Close();
        }
    }

    public void OnSelectSettings()
    {
        pauseMenuPanel.SetActive(false);
        settingsMenuPanel.SetActive(true);
    }

    void Update()
    {
        if (Input.GetButtonUp("Cancel"))
        {
            pauseHandler.SetGamePaused(false);
            print("close.");
        }
    }
}