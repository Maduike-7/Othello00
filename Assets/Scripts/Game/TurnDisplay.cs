using System.Collections;
using UnityEngine;
using TMPro;
using static CoroutineHelper;

public class TurnDisplay : MonoBehaviour
{
    TextMeshProUGUI turnText;

    IEnumerator textAnimation;
    [SerializeField] AnimationCurve textAnimationInterpolation;

    GameController gc;

    void Awake()
    {
        turnText = GetComponent<TextMeshProUGUI>();

        gc = FindObjectOfType<GameController>();
        gc.TurnPassAction += OnTurnPass;
        gc.GameOverAction += OnGameOver; 
    }

    void OnTurnPass(bool playerTurn, int numTurnsPassed)
    {
        if (numTurnsPassed > 1) return;

        string text = $"{(numTurnsPassed > 0 ? "No valid moves. " : "")}{(playerTurn ? "Player's" : "CPU's")} turn{(numTurnsPassed > 0 ? " again.": ".")}";
        AnimateText(text);
    }

    void OnGameOver()
    {
        int blackDiscCount = gc.discCount.black;
        int whiteDiscCount = gc.discCount.white;

        string text = blackDiscCount > whiteDiscCount ? "You win!" : blackDiscCount == whiteDiscCount ? "The result is a draw." : "CPU wins.";
        AnimateText(text);
    }

    void AnimateText(string text)
    {
        if (textAnimation != null)
        {
            StopCoroutine(textAnimation);
        }

        textAnimation = _AnimateText(text);
        StartCoroutine(textAnimation);
    }

    IEnumerator _AnimateText(string _text)
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