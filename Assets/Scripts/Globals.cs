using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Globals
{
    //game state
    public static bool gameOver;
    public static bool gamePaused;

    public static bool playerTurn = true;
    public static int blackDiscCount = 2;
    public static int whiteDiscCount = 2;

    //game settings
    public static bool hintsEnabled = true;
    public static bool soundEnabled = true;

    //game logic
    public static readonly int blackDiscLayer = LayerMask.NameToLayer("Black Disc");
    public static readonly int whiteDiscLayer = LayerMask.NameToLayer("White Disc");

    public const float FLIP_ANIMATION_DURATION = 0.5f;

    //CPU
    public enum CPUDifficulty
    {
        Easy,
        Normal,
        Hard
    }
    public static CPUDifficulty cpuDifficulty = CPUDifficulty.Easy;
    public static readonly int cpuDifficultyCount = System.Enum.GetNames(typeof(CPUDifficulty)).Length;


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
        cpuDifficulty = (CPUDifficulty)newValue;
        PlayerPrefs.SetInt("CPU difficulty", newValue);
    }
}