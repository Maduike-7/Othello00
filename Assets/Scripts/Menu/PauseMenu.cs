using UnityEngine;

public class PauseMenu : Menu
{
    PauseHandler pauseHandler;

    [SerializeField] GameObject pauseMenuPanel;
    [SerializeField] GameObject settingsMenuPanel;

    protected override void Awake()
    {
        base.Awake();

        pauseHandler = FindObjectOfType<PauseHandler>();
        pauseHandler.GamePauseAction += OnGamePaused;

        backgroundTransition = FindObjectOfType<BackgroundTransition>();
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

    public override void HandleBackButtonInput()
    {
        pauseHandler.SetGamePaused(false);
    }
}