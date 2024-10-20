using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MicrophoneInput : MonoBehaviour
{
    public string microphone;
    [SerializeField] AudioAnalyzer analyzer;
    [SerializeField] TMP_Dropdown microInputDropDown;


    private bool recording = false;
    private AudioSource audioSource;
    private int sampleRate;
    private readonly int buffersize = (int)Mathf.Pow(2, 13);
    private float actualRecordingLength;
    private int positionInClip = 0;


    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        microInputDropDown.AddOptions(Microphone.devices.ToList());
        microInputDropDown.value = 0;
        microphone = Microphone.devices[0];

        sampleRate = NoteManager.Instance.DefaultSamplerate;
        actualRecordingLength = .5f;


        NoteManager.Instance.MaxBMP = (int)(60 / actualRecordingLength);
    }

    public void StartStopRecording(TMP_Text _bntText)
    {
        recording = !recording;
        if (!recording)
        {
            Microphone.End(microphone);
            EventManager.TriggerEvent("StopedRecording", null);
        }
        else
        {

            StartCoroutine(GrapMicrophoneBuffer());
            StartMicrophone();
            positionInClip = 0;

            EventManager.TriggerEvent("StartedRecording", null);
        }
        NoteManager.Instance.IsRecording = recording;
        NoteManager.Instance.PlayPaused = !NoteManager.Instance.PlayPaused;
        ChangeRecordBtnText(_bntText);
    }
    private void ChangeRecordBtnText(TMP_Text _bntText)
    {
        _bntText.text = recording ? "Stop" : "Record";
    }
    void StartMicrophone()
    {
        audioSource.clip = Microphone.Start(microphone, true, 3599, sampleRate);
    }
    public IEnumerator GrapMicrophoneBuffer()
    {
        yield return new WaitUntil(() => Microphone.GetPosition(microphone) - positionInClip >= buffersize);

        AudioClip clip = audioSource.clip;
        float[] samples = AudioComponents.Instance.ExtractDataOutOfAudioClip(clip, positionInClip);
        positionInClip = Microphone.GetPosition(microphone);

        analyzer.Analyze(samples);
        StartCoroutine(GrapMicrophoneBuffer());

        //var vis = new Dictionary<string, object>
        //{
        //    { "plotting_data", new List<object> {
        //            new List<object> { 1, 1, samples.Take(500).ToArray()}
        //        }
        //    }
        //};
        //GraphPlotter.Instance.PlotGraph(vis);

    }
    public void OnMicrophoneInputChanged()
    {
        microphone = microInputDropDown.options[microInputDropDown.value].text;
        print(microphone);
    }

}
