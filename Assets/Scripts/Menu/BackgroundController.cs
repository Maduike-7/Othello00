using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static CoroutineHelper;

public class BackgroundController : MonoBehaviour
{
    [SerializeField] UserSettings userSettings;

    [Space]

    [SerializeField] Image blackBackground;
    [SerializeField] Image currentBackground;
    [SerializeField] List<Sprite> backgroundImages;

    [Space]

    [SerializeField] AnimationCurve transitionInterpolation;
    IEnumerator BackgroundTransitionCoroutine;

    void Awake()
    {
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex == 0)
        {
            FindObjectOfType<SettingsMenu>().BackgroundChangeAction += OnChangeBackgroundImage;
        }

        currentBackground.sprite = backgroundImages[userSettings.backgroundImage];
    }

    void OnChangeBackgroundImage()
    {
        if (BackgroundTransitionCoroutine != null)
        {
            StopCoroutine(BackgroundTransitionCoroutine);
        }

        BackgroundTransitionCoroutine = Fade();
        StartCoroutine(BackgroundTransitionCoroutine);
    }

    IEnumerator Fade()
    {
        float currentLerpTime = 0f, totalLerpTime = 0.5f;
        float startAlpha = blackBackground.color.a;


        while (currentLerpTime <= totalLerpTime)
        {
            float lerpProgress = transitionInterpolation.Evaluate(currentLerpTime / totalLerpTime);

            Color c = blackBackground.color;
            c.a = Mathf.Lerp(startAlpha, 1f, lerpProgress);
            blackBackground.color = c;

            yield return EndOfFrame;
            currentLerpTime += Time.deltaTime;
        }

        currentBackground.sprite = backgroundImages[userSettings.backgroundImage];
        currentLerpTime = totalLerpTime;

        while (currentLerpTime >= 0f)
        {
            float lerpProgress = transitionInterpolation.Evaluate(currentLerpTime / totalLerpTime);

            Color c = blackBackground.color;
            c.a = Mathf.Lerp(0f, 1f, lerpProgress);
            blackBackground.color = c;

            yield return EndOfFrame;
            currentLerpTime -= Time.deltaTime;
        }

        blackBackground.color = new Color(0, 0, 0, 0);
    }
}