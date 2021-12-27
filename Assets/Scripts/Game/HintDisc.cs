using System.Collections;
using UnityEngine;
using static CoroutineHelper;

public class HintDisc : MonoBehaviour
{
    Material mat;
    AnimationCurve fadeInterpolation = AnimationCurve.EaseInOut(0f, 0.5f, 1f, 0.75f);

    void Awake()
    {
        mat = GetComponent<MeshRenderer>().material;
    }

    IEnumerator Start()
    {
        float currentLerpTime = 0f, totalLerpTime = 1.5f;
        Color c = mat.color;

        while (enabled)
        {
            while (currentLerpTime < totalLerpTime)
            {
                c.a = fadeInterpolation.Evaluate(currentLerpTime / totalLerpTime);
                mat.color = c;

                yield return EndOfFrame;
                currentLerpTime += Time.deltaTime;
            }

            currentLerpTime = 0f;

            while (currentLerpTime < totalLerpTime)
            {
                c.a = fadeInterpolation.Evaluate(1 - (currentLerpTime / totalLerpTime));
                mat.color = c;

                yield return EndOfFrame;
                currentLerpTime += Time.deltaTime;
            }

            currentLerpTime = 0f;
        }
    }
}