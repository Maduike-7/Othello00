using UnityEngine;

public class AppInit : MonoBehaviour
{
    [SerializeField] UserSettings userSettings;

    void Awake()
    {
        userSettings.Load();
    }
}