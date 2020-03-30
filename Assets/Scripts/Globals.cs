using UnityEngine;

public static class Globals
{
    #region Game

    public static bool gameOver;
    public static bool inputEnabled;

    public static bool playerTurn = true;
    public static int blackDiscCount = 2;
    public static int whiteDiscCount = 2;

    public static readonly int blackDiscLayer = LayerMask.NameToLayer("Black Disc");
    public static readonly int whiteDiscLayer = LayerMask.NameToLayer("White Disc");

    public const float FlipAnimationDuration = 0.5f;
    public const float FlipAnimationDelay = 0.1f;

    #endregion
    
    #region Options

    public static bool hintsEnabled = true;
    public static bool soundEnabled = true;

    public static void ToggleHints()
    {
        hintsEnabled = !hintsEnabled;
        PlayerPrefs.SetInt("Hints", hintsEnabled ? 1 : 0);
    }

    public static void ToggleSound()
    {
        soundEnabled = !soundEnabled;
        PlayerPrefs.SetInt("Sound", soundEnabled ? 1 : 0);
    }

    public static void ChangeCPUDifficulty(int newValue)
    {
        cpuDifficulty = Mathf.Clamp(newValue, 0, MaxCPUDifficulty);
        PlayerPrefs.SetInt("CPU difficulty", cpuDifficulty);
    }

    #endregion

    #region CPU

    public static int cpuDifficulty;
    public const int MaxCPUDifficulty = 2; 

    #endregion
}