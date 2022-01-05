using UnityEngine;

public class AppInit : MonoBehaviour
{
    [SerializeField] UserSettings userSettings;

    void Awake()
    {
        Screen.fullScreen = false;
        userSettings.Load();
    }
}