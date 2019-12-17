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

    //game logic
    public static readonly int blackDiscLayer = LayerMask.NameToLayer("Black Disc");
    public static readonly int whiteDiscLayer = LayerMask.NameToLayer("White Disc");

    public const float FLIP_ANIMATION_DURATION = 0.5f;

    //game settings
    public static bool hintsEnabled = true;

    public enum CPUDifficulty
    {
        Easy,
        Normal,
        Hard
    }
    public static CPUDifficulty cpuDifficulty = CPUDifficulty.Easy;
}