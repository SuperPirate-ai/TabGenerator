using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using System;

public class AudioComponents : MonoBehaviour
{
    [SerializeField] NotesSO notes;
    private int buffersize;
    public static AudioComponents Instance;
    private float previousMaxDisplacement = Mathf.Infinity;
    private int thenidx;

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
    public bool DetectPickStroke(float[] _sample, int noteIndex)
    {
        float maxDisplacement = 0;
        for (int i = 0; i < _sample.Length; i++)
        {
            if (Math.Abs(_sample[i]) > maxDisplacement) {
                maxDisplacement = Math.Abs(_sample[i]);
            }
        }
        if (maxDisplacement < 0.01f) {
            return false;
        }


        if (maxDisplacement > previousMaxDisplacement * 1.3) {
            previousMaxDisplacement = maxDisplacement;
            thenidx = noteIndex;
            return true;
        }
        previousMaxDisplacement = maxDisplacement;
        thenidx = noteIndex;
        return false;
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
