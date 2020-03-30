using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioController : MonoBehaviour
{
    [SerializeField] AudioSource aux;
    [SerializeField] List<AudioClip> discPlaceSounds;
    [SerializeField] AudioClip discFlipSound;
    int prevSoundIndex;

    void Awake()
    {
        GameController gameController = FindObjectOfType<GameController>();

        gameController.DiscPlaceAction += PlayDiscPlaceSound;
        gameController.DiscFlipAction += PlayDiscFlipSound;
    }

    void PlayDiscPlaceSound()
    {
        int randIndex = Random.Range(0, discPlaceSounds.Count);

        //continue randomizing randIndex if the same sound were to play twice in a row
        while (randIndex == prevSoundIndex)
        {
            randIndex = Random.Range(0, discPlaceSounds.Count);
        }
        
        aux.clip = discPlaceSounds[randIndex];
        prevSoundIndex = randIndex;

        aux.Play();
    }

    //can't use AudioSource.PlayDelayed(), otherwise setting AudioSource.clip here will overwrite disc-place sound that was set in PlayDiscPlaceSound()
    //other option would be to have separate AudioSources to handle disc-place and disc-flip sfx independently
    IEnumerator PlayDiscFlipSound(float delay = 0f)
    {
        yield return new WaitForSeconds(delay);
        aux.clip = discFlipSound;
        aux.Play();
    }
}