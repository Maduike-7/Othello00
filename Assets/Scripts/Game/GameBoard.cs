using UnityEngine;

public class GameBoard : MonoBehaviour
{
    [SerializeField] UserSettings userSettings;
    [SerializeField] Texture[] boardColours;

    void Awake()
    {
        InitBoardColour();
    }

    void InitBoardColour()
    {
        var mat = GetComponent<MeshRenderer>().material;
        mat.SetTexture("_MainTex", boardColours[(int)userSettings.boardColour]);
    }
}