﻿using UnityEngine.Audio;
using UnityEngine;

[System.Serializable]
public class Sound
{

    public string name;

    public AudioClip clip;

    [Range(0f,100f)]
    public float volume;

    [Range(0f,3f)]
    public float pitch;

    public bool Loop;

    public bool isNotPlaying;

    public AudioMixerGroup output;

    [HideInInspector]
    public AudioSource source;
}