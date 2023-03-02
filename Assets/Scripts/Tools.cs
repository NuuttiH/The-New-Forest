using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Tools
{
    public static float PlayAudio(GameObject parent, AudioEvent audio)
    {
        var audioPlayer = new GameObject("AudioPlayer", typeof (AudioSource)).GetComponent<AudioSource>();
        audioPlayer.transform.position = parent.transform.position;
        audio.Play(audioPlayer);
        
        float duration = audioPlayer.clip.length*audioPlayer.pitch
        Destroy(audioPlayer.gameObject, duration);
        return duration;
    }
}
