using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MicrophoneInput : MonoBehaviour
{
    public string microphone;
    [SerializeField] AudioAnalyser analyser;
    [SerializeField] TMP_Dropdown microInputDropDown;


    private bool recording = false;
    private AudioSource audioSource;
    private int sampleRate;
    private int buffersize;
    private float actualRecordingLength;
    private int positionInClip = 0;

    void Awake()
    {
        buffersize = NoteManager.Instance.DefaultBufferSize;
    }

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
        yield return new WaitUntil(() => Microphone.GetPosition(microphone) > buffersize + positionInClip);
        AudioClip clip;
        float[] sample;
        clip = audioSource.clip;
        sample = AudioComponents.Instance.ExtractDataOutOfAudioClip(clip, positionInClip);

        positionInClip += sample.Length;

        //int[] bufidxs = new int[sample.Length];
        //for (int i = 0; i < sample.Length; i++)
        //{
        //    bufidxs[i] = i;
        //}

        //var vis = new Dictionary<string, object>
        //{
        //    { "y", bufidxs },
        //    { "xs", new float [][] { sample } }
        //};
        //audio_visualization_interface.Instance.CallVisualisation(vis);
        analyser.Analyse(sample);
        StartCoroutine(GrapMicrophoneBuffer());
    }
    public void OnMicrophoneInputChanged()
    {
        microphone = microInputDropDown.options[microInputDropDown.value].text;
        print(microphone);
    }

}
