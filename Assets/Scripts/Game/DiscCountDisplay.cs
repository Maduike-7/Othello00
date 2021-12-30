using UnityEngine;
using TMPro;

public class DiscCountDisplay : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI blackDiscCountText, whiteDiscCountText;

    void Awake()
    {
        FindObjectOfType<GameController>().ScoreUpdateAction += UpdateDiscCount;
    }

    void UpdateDiscCount(int blackCount, int whiteCount)
    {
        blackDiscCountText.text = blackCount.ToString();
        whiteDiscCountText.text = whiteCount.ToString();
    }
}