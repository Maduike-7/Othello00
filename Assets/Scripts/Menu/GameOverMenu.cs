using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameOverMenu : Menu
{
    protected override void Awake()
    {
        base.Awake();

        FindObjectOfType<GameController>().GameOverAction += OnGameOver;
        backgroundTransition = FindObjectOfType<BackgroundTransition>();
    }

    void OnGameOver()
    {
        Open();
    }

    void Update()
    {
        
    }
}