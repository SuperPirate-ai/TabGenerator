using Codice.Client.Common.GameUI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MicrophoneInput : MonoBehaviour
{
    public string microphone;
    bool recording;
    [SerializeField] AudioVisualizer visualizer;
    private AudioSource audioSource;
    private int sampleRate = 44100;
    private int buffersize = (int)Mathf.Pow(2, 13);
    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        foreach(string device in Microphone.devices) 
        {
            if(microphone == null)
                microphone = device;
        }
       print(microphone);
       StartCoroutine( UpdateMicrophone());
    }
    
    public void StartRecording()
    {
        if(recording)
        {
            StopCoroutine(UpdateMicrophone());
            recording = false;
            return;
        }
        StartCoroutine(UpdateMicrophone());
        recording = true;

    }
    public IEnumerator UpdateMicrophone()
    {
        audioSource.Stop();
        audioSource.clip = Microphone.Start(microphone, false, 1, sampleRate);

        yield return new WaitForSecondsRealtime(.1f);
        Microphone.End(microphone);
        visualizer.CalculateNote(audioSource.clip);
    }


}
