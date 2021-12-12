using UnityEngine;

[CreateAssetMenu(fileName = "New UserSettings", menuName = "Custom Data Type/User Settings")]
public class UserSettings : ScriptableObject
{
    public enum BackgroundImage
    {
        Wood,
        BlueDot,
        PinkButterfly,
        GreenStripe,
        OrangeLeaf
    }

    public enum CPUDifficulty
    {
        Easy,
        Normal,
        Hard
    }

    public BackgroundImage backgroundImage;
    public CPUDifficulty cpuDifficulty;

    public bool soundOn;
    public bool hintsOn;
}