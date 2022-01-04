using UnityEngine;

public class GameBoard : MonoBehaviour
{
    [SerializeField] UserSettings userSettings;
    [SerializeField] ColourObject[] boardColours;

    void Awake()
    {
        InitBoardColour();
    }

    void InitBoardColour()
    {
        var mat = GetComponent<MeshRenderer>().material;
        mat.color = boardColours[(int)userSettings.boardColour].value;
    }
}