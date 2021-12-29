using UnityEngine;

public class PauseMenu : Menu
{
    [SerializeField] PauseHandler pauseHandler;

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
            Close();
        }
    }

    void Update()
    {
        if (Input.GetButtonUp("Cancel"))
        {
            pauseHandler.SetGamePaused(false);
        }
    }
}