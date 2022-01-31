using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "New UserSettings", menuName = "Custom Data Type/User Settings")]
public class UserSettings : ScriptableObject
{
    public enum CPUDifficulty
    {
        Beginner,
        Intermediate,
        Advanced,
        Expert
    }

    public CPUDifficulty cpuDifficulty = 0;

    public int backgroundImage = 0;
    public int boardColour = 0;

    public bool soundOn = true;
    public bool hintsOn = true;
    public bool animationsOn = true;
    public bool clockOn = true;
}