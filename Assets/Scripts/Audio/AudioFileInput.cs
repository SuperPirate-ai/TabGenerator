using Accord.Audio;
using Accord.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioFileInput : MonoBehaviour
{
    [SerializeField] AudioClip audioClip;
    [SerializeField] AudioAnalyser analyser;

    public void StartAnalysingBtn()
    {
        float[] samples = AudioComponents.Instance.ExtractDataOutOfAudioClip(audioClip,0);
        analyser.Analyse(audioClip);
    }
}
