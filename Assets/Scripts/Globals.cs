using System.Collections;
using System.Collections.Generic;
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

    public const float FLIP_ANIMATION_DURATION = 0.5f;
    public const float FLIP_ANIMATION_DELAY = 0.1f;

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
        cpuDifficulty = Mathf.Clamp(newValue, 0, MAX_CPU_DIFFICULTY);
        PlayerPrefs.SetInt("CPU difficulty", cpuDifficulty);
    }

    #endregion

    #region CPU

    public static int cpuDifficulty;
    public const int MAX_CPU_DIFFICULTY = 2; 

    #endregion
}