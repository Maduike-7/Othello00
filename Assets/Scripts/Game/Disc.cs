using System.Collections;
using UnityEngine;
using static CoroutineHelper;

public class Disc : MonoBehaviour
{
    readonly AnimationCurve flipAnimationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    int BlackDiscLayer => LayerMask.NameToLayer("Black Disc");
    int WhiteDiscLayer => LayerMask.NameToLayer("White Disc");
    public bool IsBlack => gameObject.layer == BlackDiscLayer;

    public void FlipUponAxis(Vector3 flipAxis, float flipDuration = 0f, float flipDelay = 0f)
    {
        //toggle layer (white <-> black)
        gameObject.layer = IsBlack ? WhiteDiscLayer : BlackDiscLayer;

        Quaternion startRotation = Quaternion.AngleAxis(IsBlack ? 180 : 0, flipAxis);
        Quaternion endRotation = Quaternion.AngleAxis(IsBlack ? 0 : 180, flipAxis);

        //if this disc is visible in scene, start rotate animation
        if (gameObject.activeSelf)
        {
            StartCoroutine(Rotate(startRotation, endRotation, flipDuration, flipDelay));
        }
        //otherwise just set its rotation
        else
        {
            transform.localRotation = Quaternion.AngleAxis(IsBlack ? 0 : 180, flipAxis);
        }
    }

    //lerp rotation from <startRot> to <endRot>
    IEnumerator Rotate(Quaternion startRot, Quaternion endRot, float flipDuration, float flipDelay)
    {
        Vector3 originalPos = transform.localPosition;
        float currentLerpTime = 0f;

        yield return WaitForSeconds(flipDelay);

        if (flipDuration <= 0)
        {
            transform.localRotation = endRot;
            yield break;
        }

        while (currentLerpTime <= flipDuration)
        {
            yield return EndOfFrame;
            currentLerpTime += Time.deltaTime;
            float t = currentLerpTime / flipDuration;

            //set rotation
            transform.localRotation = Quaternion.Lerp(startRot, endRot, flipAnimationCurve.Evaluate(t));

            //set position
            Vector3 newPos = transform.position;
            newPos.z = -Mathf.Sin(Mathf.PI * t) / 2;
            transform.position = newPos;
        }

        transform.localPosition = originalPos;
        transform.localRotation = endRot;
    }
}