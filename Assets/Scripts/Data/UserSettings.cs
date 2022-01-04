using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "New UserSettings", menuName = "Custom Data Type/User Settings")]
public class UserSettings : ScriptableObject
{
    public enum BackgroundImage
    {
        Wood,
        BlueBubble,
        PinkButterfly,
        GreenStripe,
        OrangeLeaf
    }

    public enum BoardColour
    {
        Green,
        Blue,
        Orange,
        Purple,
        Grey
    }

    public enum CPUDifficulty
    {
        Beginner,
        Intermediate,
        Advanced
    }

    public BackgroundImage backgroundImage = 0;
    public BoardColour boardColour = 0;

    public CPUDifficulty cpuDifficulty = 0;

    public bool soundOn = true;
    public bool hintsOn = true;
}