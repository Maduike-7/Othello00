using System.Collections;
using UnityEngine;
using TMPro;
using static CoroutineHelper;

public class TurnDisplay : MonoBehaviour
{
    TextMeshProUGUI turnText;
    IEnumerator textAnimation;

    void Awake()
    {
        turnText = GetComponent<TextMeshProUGUI>();
        FindObjectOfType<GameController>().TurnPassAction += UpdateTurnText;
    }

    void UpdateTurnText(bool playerTurn, int numTurnsPassed)
    {
        string text = $"{playerTurn}, {numTurnsPassed}";

        if (numTurnsPassed > 0)
        {
            //to-do
        }
        if (playerTurn)
        {
            //to-do
        }

        if (textAnimation != null)
        {
            StopCoroutine(textAnimation);
        }

        textAnimation = AnimateTurnText(text);
        StartCoroutine(textAnimation);
    }

    IEnumerator AnimateTurnText(string _text)
    {
        turnText.text = _text;
        yield return null;
    }
}