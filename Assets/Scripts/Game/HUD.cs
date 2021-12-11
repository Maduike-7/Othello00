using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static Globals;

public class HUD : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI gameStateDisplay, blackCountDisplay, whiteCountDisplay;
    [SerializeField] Button optionsButton, mainMenuButton;

    void Awake()
    {
        FindObjectOfType<GameController>().ScoreUpdateAction += UpdateHUD;
        FindObjectOfType<GameController>().GameOverAction += OnGameOver;
    }

    void UpdateHUD()
    {
        //update disc count display
        blackCountDisplay.text = blackDiscCount.ToString();
        whiteCountDisplay.text = whiteDiscCount.ToString();

        //update game state displays based on whose turn it is
        gameStateDisplay.text = (playerTurn ? "Your " : "CPU's ") + "turn.";
        gameStateDisplay.color = playerTurn ? Color.black : Color.white;
    }

    void OnGameOver()
    {
        //update game state displays based on who has more discs
        gameStateDisplay.text = "Game over.\n" + (blackDiscCount > whiteDiscCount ? "You win!" : (blackDiscCount < whiteDiscCount ? "CPU wins." : "Tie game"));
        gameStateDisplay.color = blackDiscCount > whiteDiscCount ? Color.black : (blackDiscCount < whiteDiscCount ? Color.white : Color.gray);

        optionsButton.gameObject.SetActive(false);
        mainMenuButton.gameObject.SetActive(true);
    }
}