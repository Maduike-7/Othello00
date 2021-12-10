using System.Collections;
using UnityEngine;
using static Globals;
using static CoroutineHelper;

public class Disc : MonoBehaviour
{
    readonly AnimationCurve flipAnimationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);    

    void OnEnable()
    {
        //increment disc count based on whose turn it is
        if (playerTurn) { blackDiscCount++; }
        else { whiteDiscCount++; }
    }

    public void FlipUponAxis(Vector3 flipAxis, float flipDelay = 0f)
    {
        //toggle layer (white <-> black)
        gameObject.layer = gameObject.layer == blackDiscLayer ? whiteDiscLayer : blackDiscLayer;

        //if this disc is visible in scene, start rotate animation
        if (gameObject.activeSelf)
        {
            Quaternion startRotation = Quaternion.AngleAxis(gameObject.layer == blackDiscLayer ? 180 : 0, flipAxis);
            Quaternion endRotation = Quaternion.AngleAxis(gameObject.layer == blackDiscLayer ? 0 : 180, flipAxis);

            StartCoroutine(Rotate(startRotation, endRotation, flipDelay));
        }
        //otherwise just set its rotation
        else
        {
            transform.localRotation = Quaternion.AngleAxis(gameObject.layer == blackDiscLayer ? 0 : 180, flipAxis);
        }
    }

    //lerp rotation from <startRot> to <endRot>
    IEnumerator Rotate(Quaternion startRot, Quaternion endRot, float flipDelay)
    {
        Vector3 originalPos = transform.localPosition;
        float currentLerpTime = 0f;

        yield return WaitForSeconds(flipDelay);

        while (currentLerpTime <= FlipAnimationDuration)
        {
            yield return EndOfFrame;
            currentLerpTime += Time.deltaTime;
            float t = currentLerpTime / FlipAnimationDuration;

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