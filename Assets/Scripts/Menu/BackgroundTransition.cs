using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using static CoroutineHelper;

public class BackgroundTransition : MonoBehaviour
{
    Image backgroundImage;
    [SerializeField] AnimationCurve translateInterpolation;

    void Awake()
    {
        backgroundImage = GetComponentInChildren<Image>();
    }

    IEnumerator Start()
    {
        yield return Fade(1f, 0f, 1f);
    }

    public IEnumerator Fade(float startAlpha, float endAlpha, float lerpDuration)
    {
        float currentLerpTime = 0f;

        backgroundImage.raycastTarget = true;

        while (currentLerpTime <= lerpDuration)
        {
            float lerpProgress = translateInterpolation.Evaluate(currentLerpTime / lerpDuration);

            Color c = backgroundImage.color;
            c.a = Mathf.Lerp(startAlpha, endAlpha, lerpProgress);
            backgroundImage.color = c;

            yield return EndOfFrame;
            currentLerpTime += Time.deltaTime;
        }

        backgroundImage.color = new Color(0, 0, 0, endAlpha);
        backgroundImage.raycastTarget = false;
    }
}