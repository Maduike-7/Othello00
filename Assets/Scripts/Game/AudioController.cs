using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioController : MonoBehaviour
{
    [SerializeField] AudioSource aux;
    [SerializeField] List<AudioClip> discPlaceSounds;
    [SerializeField] AudioClip discFlipSound;
    int prevSoundIndex;

    public void PlayDiscPlaceSound()
    {
        //get random number to represent index of sounds[] to play
        int randIndex = Random.Range(0, discPlaceSounds.Count);

        //continue randomizing if the same sound were to play twice in a row
        while (randIndex == prevSoundIndex)
        {
            randIndex = Random.Range(0, discPlaceSounds.Count);
        }

        //update audio source's clip; play sound
        aux.clip = discPlaceSounds[randIndex];
        aux.Play();

        //update prevSoundIndex
        prevSoundIndex = randIndex;
    }

    //can't use AudioSource.PlayDelayed(); has to be a coroutine
    //otherwise setting AudioSource.clip here will overwrite disc-place sound that was set in PlayDiscPlaceSound()
    //other option would be to have separate AudioSources to handle disc-place and disc-flip sfx independently
    public IEnumerator PlayDiscFlipSound(float delay = 0f)
    {
        yield return new WaitForSeconds(delay);
        aux.clip = discFlipSound;
        aux.Play();
    }
}