using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Menu : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI titleText;
    protected Canvas thisMenu;

    protected virtual void Awake()
    {
        thisMenu = GetComponent<Canvas>();
    }

    void Update()
    {
        if (thisMenu.enabled)
        {
            AnimateTitleText();
        }
    }

    //animate title opacity from [0.2, 1.0] over a cosine wave
    void AnimateTitleText()
    {
        //Color c = titleText.color;
        VertexGradient grad = titleText.colorGradient;

        float topAlpha = 0.8f * Mathf.Cos(Time.timeSinceLevelLoad) / 2 + 0.6f;
        float bottomAlpha = 0.8f * Mathf.Sin(Time.timeSinceLevelLoad) / 2 + 0.6f;

        grad.topLeft.a = topAlpha;
        grad.topRight.a = topAlpha;
        grad.bottomLeft.a = bottomAlpha;
        grad.bottomRight.a = bottomAlpha;

        titleText.colorGradient = grad;
    }

    protected void OpenMenu(Canvas menu)
    {
        menu.enabled = true;
    }

    protected void CloseMenu(Canvas menu)
    {
        menu.enabled = false;
    }

    public void SwitchMenu(Canvas menu)
    {
        OpenMenu(menu);
        CloseMenu(thisMenu);
    }

    public void LoadScene(int sceneIndex)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneIndex);
    }
}