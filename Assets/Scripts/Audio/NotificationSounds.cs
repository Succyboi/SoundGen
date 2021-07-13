using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class NotificationSounds : MonoBehaviour
{
    public AudioClip[] sounds;

    [HideInInspector] public static NotificationSounds instance;
    private AudioSource source;

    private void Start()
    {
        instance = this;
        source = GetComponent<AudioSource>();
    }

    public void PlaySound(NotificationSound sound)
    {
        if(sounds[(int)sound] != null)
        {
            source.PlayOneShot(sounds[(int)sound]);
        }
    }
}

public enum NotificationSound
{
    Good,
    Bad
}