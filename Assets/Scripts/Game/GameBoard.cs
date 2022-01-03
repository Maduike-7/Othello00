using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameBoard : MonoBehaviour
{
    [SerializeField] UserSettings userSettings;

    void Awake()
    {
        InitBoardColour();
    }

    void InitBoardColour()
    {
        MeshRenderer mesh = GetComponent<MeshRenderer>();
        mesh.material = mesh.materials[(int)userSettings.boardColour];
    }
}