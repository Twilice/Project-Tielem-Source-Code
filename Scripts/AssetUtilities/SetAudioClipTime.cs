using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SetAudioClipTime : MonoBehaviour
{
    public float startTime = 0;
    public float endTime = 1;

    void Awake()
    {
        var audioSource = GetComponent<AudioSource>();
        audioSource.time = startTime;
        audioSource.SetScheduledEndTime(AudioSettings.dspTime+(endTime-startTime));
    }
}
