using System.IO;
using UnityEngine;

public static class FileHandler
{
    static readonly string settingsDirectoryPath = Path.Combine(Application.persistentDataPath, "Settings");
    static readonly string settingsFilePath = Path.Combine(settingsDirectoryPath, "usersettings.json");

    public static void Load(this UserSettings userSettings)
    {
        Directory.CreateDirectory(settingsDirectoryPath);

        try
        {
            var json = File.ReadAllText(settingsFilePath);
            JsonUtility.FromJsonOverwrite(json, userSettings);
        }
        catch (FileNotFoundException)
        {
            File.Create(settingsFilePath);

            var _userSettings = ScriptableObject.CreateInstance<UserSettings>();
            userSettings = _userSettings;
        }
    }

    public static void Save(this UserSettings userSettings)
    {
        var json = JsonUtility.ToJson(userSettings, true);
        File.WriteAllText(settingsFilePath, json);
    }
}