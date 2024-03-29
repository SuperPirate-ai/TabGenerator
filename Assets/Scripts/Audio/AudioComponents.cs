using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using System;
using CL = CustomLogger;
using UnityEditor.Experimental.GraphView;

public class AudioComponents : MonoBehaviour
{
    [SerializeField] NotesSO notes;
    private int buffersize;
    public static AudioComponents Instance;
    public float previousMaxDisplacement = Mathf.Infinity;
    private string thenname;
    public int earlyreturnCounter = 0;

    private void Awake()
    {
        if (Instance != null) Destroy(this);

        Instance = this;
    }

    private void Start()
    {
        buffersize = NoteManager.Instance.DefaultBufferSize;
    }
    private void Update()
    {
        if (NoteManager.Instance.PlayPaused)
        {
            previousMaxDisplacement = Mathf.Infinity;
        }
    }
    public float[] ExtractDataOutOfAudioClip(AudioClip _clip, int _positionInClip)
    {
        float[] sample = new float[buffersize];
        _clip.GetData(sample, _positionInClip);
        return sample;
    }
    public bool DetectPickStroke(float maxDisplacement, string noteName)
    {
        earlyreturnCounter = 0;
        if (maxDisplacement < 0.001f) {
            CL.Log("too low level");
            return false;
        }
        CL.Log("high enough level");

        if (thenname != noteName) {
            previousMaxDisplacement = maxDisplacement;
            thenname = noteName;
            CL.Log("note change");
            return true;
        }
        CL.Log("no note change");

        if (maxDisplacement > previousMaxDisplacement * 3) {
            previousMaxDisplacement = maxDisplacement;
            thenname = noteName;
            CL.Log("enough relative level");
            return true;
        }
        CL.Log("not enough relative level");
        previousMaxDisplacement = maxDisplacement;
        thenname = noteName;
        return false;
    }

    public void ListenForEarlyReturn() {
        if (earlyreturnCounter > 0)
        {
            CL.Log("resetting previousMaxDisplacement");
            previousMaxDisplacement = 0;
        }
        earlyreturnCounter++;
    }

    public float[] FFT(float[] _data)
    {
        float[] fft = new float[_data.Length];
        float[] phase = new float[_data.Length];
        System.Numerics.Complex[] fftComplex = new System.Numerics.Complex[_data.Length];

        for (int i = 0; i < _data.Length; i++)
        {
            fftComplex[i] = new System.Numerics.Complex(_data[i], 0.0);
        }

        Accord.Math.FourierTransform.FFT(fftComplex, Accord.Math.FourierTransform.Direction.Forward);

        for (int i = 0; i < _data.Length; i++)
        {
            fft[i] = (float)fftComplex[i].Magnitude;
            phase[i] = (float)fftComplex[i].Phase;
        }

        return fft;
    }
}
