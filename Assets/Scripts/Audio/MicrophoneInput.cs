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
    private int sampleRate;
    private int buffersize = (int)Mathf.Pow(2, 13);
    void Start()
    {
        ////Test
        //microphone = "GT-1";
        ////
        audioSource = GetComponent<AudioSource>();
        microInputDropDown.AddOptions(Microphone.devices.ToList());
        sampleRate = NoteManager.Instance.defaultSamplerate;
    }
    
    public void StartRecording(TMP_Text _bntText)
    {
        recording = !recording;
        if (!recording)
        {
            Microphone.End(microphone);
            NoteManager.Instance.PlayPaused = true;
        }
        else
        {
            NoteManager.Instance.PlayPaused = false;
            StartCoroutine(UpdateMicrophone());
        }
        _bntText.text = recording ? "Stop" : "Record";
    }
    public IEnumerator UpdateMicrophone()
    {
        audioSource.Stop();
        if(!recording) yield break;

        audioSource.clip = Microphone.Start(microphone, false, 1, sampleRate);
        yield return new WaitForSecondsRealtime(.5f);
        Microphone.End(microphone);
        visualizer.CalculateNote(audioSource.clip);
        StartCoroutine(UpdateMicrophone());
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
