using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Tools
{
    public static float PlayAudio(GameObject parent, AudioEvent audio, bool dontDestroyOnLoad = false)
    {
        var audioPlayer = new GameObject("AudioPlayer", typeof (AudioSource)).GetComponent<AudioSource>();
        if(parent == null) parent = Camera.main.gameObject;
        audioPlayer.transform.position = parent.transform.position;
        audio.Play(audioPlayer);
        
        float duration = audioPlayer.clip.length*audioPlayer.pitch;
        if(dontDestroyOnLoad) GameObject.DontDestroyOnLoad(audioPlayer.gameObject);
        UnityEngine.Object.Destroy(audioPlayer.gameObject, duration);
        
        Debug.Log($"Tools.PlayAudio({parent.name}, {audio.name}, {dontDestroyOnLoad})");
        return duration;
    }
}
