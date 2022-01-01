using System;
using UnityEngine;
using UnityEngine.UI;

public class PauseHandler : MonoBehaviour
{
    [SerializeField] Button pauseButton;

    public event Action<bool> GamePauseAction;
    bool isPaused;

    void Awake()
    {
        FindObjectOfType<GameController>().GameOverAction += OnGameOver;
        GamePauseAction += OnGamePaused;
    }

    void Update()
    {
        if (Input.GetButtonUp("Cancel"))
        {
            SetGamePaused(!isPaused);
        }
    }

    public void SetGamePaused(bool pauseState)
    {
        GamePauseAction?.Invoke(pauseState);
    }

    void OnGamePaused(bool state)
    {
        isPaused = state;
    }

    void OnGameOver()
    {
        pauseButton.enabled = false;
        enabled = false;
    }
}