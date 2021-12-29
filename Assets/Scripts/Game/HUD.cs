using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUD : MonoBehaviour
{
    GameController gc;
    [SerializeField] TextMeshProUGUI gameStateDisplay, blackCountDisplay, whiteCountDisplay;
    [SerializeField] Button optionsButton, mainMenuButton;

    void Awake()
    {
        gc = FindObjectOfType<GameController>();

        gc.ScoreUpdateAction += UpdateHUD;
        gc.GameOverAction += OnGameOver;
    }

    void UpdateHUD()
    {
        //update disc count display
        blackCountDisplay.text = gc.discCount.black.ToString();
        whiteCountDisplay.text = gc.discCount.white.ToString();

        //update game state displays based on whose turn it is
        gameStateDisplay.text = (gc.PlayerTurn ? "Your " : "CPU's ") + "turn.";
        gameStateDisplay.color = gc.PlayerTurn ? Color.black : Color.white;
    }

    void OnGameOver()
    {
        //update game state displays based on who has more discs
        gameStateDisplay.text = "Game over.\n" + (gc.discCount.black > gc.discCount.white ? "You win!" : (gc.discCount.black < gc.discCount.white ? "CPU wins." : "Tie game"));
        gameStateDisplay.color = gc.discCount.black > gc.discCount.white ? Color.black : (gc.discCount.black < gc.discCount.white ? Color.white : Color.gray);

        optionsButton.gameObject.SetActive(false);
        mainMenuButton.gameObject.SetActive(true);
    }
}