using UnityEngine;
using TMPro;

public class DiscCountDisplay : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI blackDiscCountText, whiteDiscCountText;

    void Awake()
    {
        FindObjectOfType<GameController>().ScoreUpdateAction += UpdateDiscCount;
    }

    void UpdateDiscCount(int blackDiscCount, int whiteDiscCount)
    {
        blackDiscCountText.text = blackDiscCount.ToString();
        whiteDiscCountText.text = whiteDiscCount.ToString();
    }
}