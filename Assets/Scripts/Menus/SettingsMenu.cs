using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Globals;

public class SettingsMenu : Menu
{
    public void OnCPUDifficultyChanged(int difficulty)
    {
        cpuDifficulty = (CPUDifficulty)difficulty;
    }
}