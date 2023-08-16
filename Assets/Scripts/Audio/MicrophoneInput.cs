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
        ShowSpectrum();
        Microphone.End(microphone);
        visualizer.CalculateNote(audioSource.clip);
    }
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
