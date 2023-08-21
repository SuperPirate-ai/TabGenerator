using Codice.Client.Common.GameUI;
using System;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MicrophoneInput : MonoBehaviour
{
    public string microphone;
    [SerializeField] AudioVisualizer visualizer;
    [SerializeField] TMP_Dropdown microInputDropDown;
    [SerializeField] Transform[] audioSpectrumObjects;
    [SerializeField] float heightMultiplier;
    [SerializeField] float lerpTime = 1;

    private bool recording = false;
    private AudioSource audioSource;
    private int sampleRate = 44100;
    private int buffersize = (int)Mathf.Pow(2, 13);
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        microInputDropDown.AddOptions(Microphone.devices.ToList());
    }
    
    public void StartRecording()
    {
        if(recording)
        {
            StopCoroutine(UpdateMicrophone());
            NoteManager.Instance.PlayPaused = true;
            recording = false;
            return;
        }
        NoteManager.Instance.PlayPaused = false;
        StartCoroutine(UpdateMicrophone());
        recording = true;

    }
    public IEnumerator UpdateMicrophone()
    {
        audioSource.Stop();
        audioSource.clip = Microphone.Start(microphone, false, 1, sampleRate);

        yield return new WaitForSecondsRealtime(.5f);
        //TrimAudioClip();
        //ShowSpectrum();
        Microphone.End(microphone);
        visualizer.CalculateNote(audioSource.clip);
        if (!recording) StopCoroutine(UpdateMicrophone());
        StartCoroutine(UpdateMicrophone());
    }
    //void TrimAudioClip()
    //{
    //    AudioClip origClip = audioSource.clip;
    //    var origClipSmaples = new float[origClip.samples];
    //    int numOfClips = (int)(origClip.length * 10);
    //    AudioClip[] clips = new AudioClip[numOfClips];
    //    float cliplenght = .1f;

    //    origClip.GetData(origClipSmaples, 0);

    //    float startPosSec = 0;
    //    for (int i = 0; i < numOfClips; i++)
    //    {
    //        int startsample = (int)(startPosSec * origClip.frequency);
    //        int newLenght = (int)(cliplenght * origClip.frequency);

    //        var newClipSamples = origClipSmaples.Skip(startsample).Take(newLenght).ToArray();
    //        AudioClip resClip = AudioClip.Create(origClip.name + i, newClipSamples.Length, origClip.channels, origClip.frequency, false);
    //        resClip.SetData(newClipSamples, 0);
    //        clips[i] = resClip;
    //        startPosSec += .1f;
    //    }
    //    StartCoroutine(Wait(clips));
    //}
    //IEnumerator Wait(AudioClip[] _clips)
    //{
    //    foreach (var item in _clips)
    //    {
    //        yield return new WaitUntil(()=> !audioSource.isPlaying);
    //        audioSource.clip = item;
    //        audioSource.Play();
    //    }
    //}
    void ShowSpectrum()
    {
        float[] samples = new float[512];
        audioSource.GetSpectrumData(samples, 0, FFTWindow.Hanning);
        for (int i = 0; i < audioSpectrumObjects.Length; i++)
        {
            Vector3 locScaleCube = audioSpectrumObjects[i].localScale;
            audioSpectrumObjects[i].localScale = new Vector3(locScaleCube.x, samples[i] * heightMultiplier);
        
        }
    }
    public void OnMicrophoneInputChanged()
    {
        microphone = microInputDropDown.options[microInputDropDown.value].text;
        print(microphone);


    }


}
