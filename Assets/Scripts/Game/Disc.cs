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

    public void FlipUponAxis(Vector3 flipAxis)
    {
        //change layer (white <-> black)
        gameObject.layer = gameObject.layer == blackDiscLayer ? whiteDiscLayer : blackDiscLayer;

        //if this disc is visible in scene, start rotate animation
        if (gameObject.activeSelf)
        {
            StartCoroutine(Rotate(Quaternion.AngleAxis(gameObject.layer == blackDiscLayer ? 0 : -180, flipAxis)));
        }
        //otherwise just set its rotation
        else
        {
            transform.localRotation = Quaternion.AngleAxis(gameObject.layer == blackDiscLayer ? 0 : -180, flipAxis);
        }
    }

    IEnumerator Rotate(Quaternion endRot)
    {
        Vector3 originalPos = transform.localPosition;
        Quaternion startRot = transform.localRotation;
        float currentLerpTime = 0f;

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