using UnityEngine;

public static class Globals
{
    #region Game

    public static bool inputEnabled;

    public static bool playerTurn = true;
    public static int blackDiscCount = 2;
    public static int whiteDiscCount = 2;

    public static readonly int blackDiscLayer = LayerMask.NameToLayer("Black Disc");
    public static readonly int whiteDiscLayer = LayerMask.NameToLayer("White Disc");

    public const float FlipAnimationDuration = 0.5f;
    public const float FlipAnimationDelay = 0.1f;

    #endregion
}