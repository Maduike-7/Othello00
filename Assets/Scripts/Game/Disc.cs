using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Globals;

public class Disc : MonoBehaviour
{
    AnimationCurve flipAnimationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    void OnEnable()
    {
        if (playerTurn) { blackDiscCount++; }
        else { whiteDiscCount++; }
    }

    public void FlipUponAxis(Vector3 flipAxis, float flipDelay = 0f)
    {
        //change layer (white <-> black)
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
            transform.localRotation = Quaternion.AngleAxis(gameObject.layer == blackDiscLayer ? 0 : -180, flipAxis);
        }
    }

    IEnumerator Rotate(Quaternion startRot, Quaternion endRot, float flipDelay)
    {
        Vector3 originalPos = transform.localPosition;
        float currentLerpTime = 0f;

        yield return new WaitForSeconds(flipDelay);

        while (currentLerpTime <= FLIP_ANIMATION_DURATION)
        {
            float t = currentLerpTime / FLIP_ANIMATION_DURATION;
            transform.localRotation = Quaternion.Lerp(startRot, endRot, flipAnimationCurve.Evaluate(t));

            Vector3 newPos = transform.position;
            newPos.z = -Mathf.Sin(Mathf.PI * t) / 2;
            transform.position = newPos;

            currentLerpTime += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = originalPos;
        transform.localRotation = endRot;
    }
}