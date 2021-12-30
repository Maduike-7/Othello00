using System.Collections;
using UnityEngine;
using TMPro;
using static CoroutineHelper;

public class TurnDisplay : MonoBehaviour
{
    TextMeshProUGUI turnText;

    IEnumerator textAnimation;
    [SerializeField] AnimationCurve textAnimationInterpolation;

    void Awake()
    {
        turnText = GetComponent<TextMeshProUGUI>();
        FindObjectOfType<GameController>().TurnPassAction += UpdateTurnText;
    }

    void UpdateTurnText(bool playerTurn, int numTurnsPassed)
    {
        string text = $"{(numTurnsPassed > 0 ? "No valid moves.\n" : "")}{(playerTurn ? "Player's" : "CPU's")} turn{(numTurnsPassed > 0 ? " again.": ".")}";

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

        float currentLerpTime = 0f, totalLerpTime = 0.5f;

        while (currentLerpTime < totalLerpTime)
        {
            Color c = turnText.color;
            c.a = textAnimationInterpolation.Evaluate(currentLerpTime / totalLerpTime);
            turnText.color = c;

            yield return EndOfFrame;
            currentLerpTime += Time.deltaTime;
        }
    }
}