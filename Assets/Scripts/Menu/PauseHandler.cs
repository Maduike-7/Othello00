using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using static CoroutineHelper;

public class PauseHandler : MonoBehaviour
{
    [SerializeField] Button pauseButton;

    public event Action<bool> GamePauseAction;
    bool isPaused;

    IEnumerator disablePauseCoroutine;

    void Awake()
    {
        GamePauseAction += OnGamePaused;

        GameController gc = FindObjectOfType<GameController>();
        gc.DiscFlipAction += OnDiscFlip;
        gc.GameOverAction += OnGameOver;
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

    void OnDiscFlip(float flipDuration)
    {
        if (disablePauseCoroutine != null)
        {
            StopCoroutine(disablePauseCoroutine);
        }

        disablePauseCoroutine = DisablePause(flipDuration);
        StartCoroutine(disablePauseCoroutine);
    }

    IEnumerator DisablePause(float duration)
    {
        enabled = false;
        pauseButton.enabled = false;

        yield return WaitForSeconds(duration);

        enabled = true;
        pauseButton.enabled = true;
    }

    void OnGameOver()
    {
        pauseButton.enabled = false;
        enabled = false;
    }
}