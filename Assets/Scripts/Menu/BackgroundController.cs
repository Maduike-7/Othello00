using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static CoroutineHelper;

public class BackgroundController : MonoBehaviour
{
    [SerializeField] UserSettings userSettings;

    [Space]

    [SerializeField] Image currentBackground;
    [SerializeField] Image nextBackground;

    [Space]

    [SerializeField] List<Image> backgroundImages;

    IEnumerator backgroundFadeCoroutine;

    void Awake()
    {
        FindObjectOfType<OptionsMenu>().BackgroundChangeAction += OnChangeBackgroundImage;
    }

    void OnChangeBackgroundImage()
    {
        if (backgroundFadeCoroutine != null)
        {
            StopCoroutine(backgroundFadeCoroutine);
        }

        backgroundFadeCoroutine = FadeBackgroundImage();
        StartCoroutine(backgroundFadeCoroutine);
    }

    IEnumerator FadeBackgroundImage()
    {
        float currentTime = 0f, totalLerpTime = 1f;

        currentBackground.sprite = nextBackground.sprite;

        nextBackground.enabled = true;
        nextBackground.sprite = backgroundImages[(int)userSettings.backgroundImage].sprite;

        while (currentTime < totalLerpTime)
        {
            float alpha = currentTime / totalLerpTime;

            Color currentBackgroundColour = currentBackground.color;
            currentBackgroundColour.a = 1f - alpha;
            currentBackground.color = currentBackgroundColour;

            Color nextBackgroundColour = nextBackground.color;
            nextBackgroundColour.a = alpha;
            nextBackground.color = nextBackgroundColour;

            currentTime += Time.deltaTime;
            yield return EndOfFrame;
        }

        currentBackground.sprite = nextBackground.sprite;
        currentBackground.color = Color.white;

        nextBackground.enabled = false;
    }
}