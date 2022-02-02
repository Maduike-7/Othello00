using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static CoroutineHelper;

public class AudioController : MonoBehaviour
{
    [SerializeField] UserSettings userSettings;

    [Space]

    [SerializeField] AudioSource aux;
    [SerializeField] List<AudioClip> discPlaceSounds;
    [SerializeField] AudioClip discFlipSound;

    void Awake()
    {
        GameController gameController = FindObjectOfType<GameController>();

        gameController.DiscPlaceAction += OnDiscPlace;
        gameController.DiscFlipAction += OnDiscFlip;
    }

    void OnDiscPlace()
    {
        if (!userSettings.soundOn) return;

        aux.clip = discPlaceSounds[Random.Range(0, discPlaceSounds.Count)];
        aux.Play();
    }

    void OnDiscFlip(float delay)
    {
        StartCoroutine(PlayDiscPlaceSound(delay));
    }

    //can't use AudioSource.PlayDelayed(), otherwise setting AudioSource.clip here will overwrite disc-place sound that was set in PlayDiscPlaceSound()
    //other option would be to have separate AudioSources to handle disc-place and disc-flip sfx independently
    IEnumerator PlayDiscPlaceSound(float delay)
    {
        yield return WaitForSeconds(delay);
        aux.clip = discFlipSound;

        if (userSettings.soundOn)
            aux.Play();
    }
}