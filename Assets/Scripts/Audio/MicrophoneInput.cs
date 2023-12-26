using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MicrophoneInput : MonoBehaviour
{
    public string microphone;
    [SerializeField] AudioAnalyser analyser;
    [SerializeField] TMP_Dropdown microInputDropDown;
    [SerializeField] Transform[] audioSpectrumObjects;
    [SerializeField] float heightMultiplier;

    private bool recording = false;
    private AudioSource audioSource;
    private int sampleRate;
    private readonly int buffersize = (int)Mathf.Pow(2, 13);
    private float actualRecordingLength;
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
            NoteManager.Instance.PlayPaused = true;
        }
        else
        {
            NoteManager.Instance.PlayPaused = false;
            StartCoroutine(UpdateMicrophone());
        }
        ChangeRecordBtnText(_bntText);
    }
    private void ChangeRecordBtnText(TMP_Text _bntText)
    {
        _bntText.text = recording ? "Stop" : "Record";
    }
    public IEnumerator UpdateMicrophone()
    {
        audioSource.Stop();
        if (!recording) yield break;
        audioSource.clip = Microphone.Start(microphone, false, 1, sampleRate);
        yield return new WaitForSecondsRealtime(actualRecordingLength);
        Microphone.End(microphone);

        AudioClip clip = audioSource.clip;
        analyser.Analyse(clip);
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
