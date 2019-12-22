using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioController : MonoBehaviour
{
    [SerializeField] AudioSource aux;
    [SerializeField] List<AudioClip> sounds;
    int prevSoundIndex;

    public void PlayRandomSound()
    {
        //get random number to represent index of sounds[] to play
        int randIndex = Random.Range(0, sounds.Count);

        //continue randomizing if the same sound were to play twice in a row
        while (randIndex == prevSoundIndex)
        {
            randIndex = Random.Range(0, sounds.Count);
        }

        //update audio source's clip; play sound
        aux.clip = sounds[randIndex];
        aux.Play();

        //update prevSoundIndex
        prevSoundIndex = randIndex;
    }
}