using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using static Globals;

public class HUD : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI gameStateDisplay, blackCountDisplay, whiteCountDisplay;

    public void UpdateHUD()
    {
        //update disc count display
        blackCountDisplay.text = blackDiscCount.ToString();
        whiteCountDisplay.text = whiteDiscCount.ToString();

        //update game state display depending on:
        //if the game isn't over, whose turn it is
        //if the game is over, who has more discs
        //(lol)
        gameStateDisplay.text = !gameOver ? ((playerTurn ? "Your " : "CPU's ") + "turn.") : "Game over.\n" + (blackDiscCount > whiteDiscCount ? "You win!" : (blackDiscCount < whiteDiscCount ? "CPU wins." : "Tie game"));
        gameStateDisplay.color = !gameOver ? (playerTurn ? Color.black : Color.white) : blackDiscCount > whiteDiscCount ? Color.black : (blackDiscCount < whiteDiscCount ? Color.white : Color.gray);
    }
}