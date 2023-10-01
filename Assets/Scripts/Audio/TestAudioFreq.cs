using Accord.Math;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class TestAudioFreq : MonoBehaviour
{
    string microphoneName;
    [SerializeField] AudioSource audioSource;
    [SerializeField] int sampleRate;
    private double[] fftReal;
    
    int numberOfSmaples = 8192;
    int buffersize = (int)Math.Pow(2, 14);
    void Start()
    {
        microphoneName = Microphone.devices[0];
        fftReal = new double[buffersize/2];
        Record();
    }
    float timeElapsed = 0f;
    void  Record()
    {
        timeElapsed += Time.deltaTime;
        audioSource.clip = null;

       audioSource.clip = Microphone.Start(microphoneName, false, 1, sampleRate);
        // while (Microphone.IsRecording(microphoneName) && timeElapsed < 1.5f) { }
       
        StartCoroutine(WaitForFixedUpdateAndCallRecord());
        //
        
    }
    IEnumerator WaitForFixedUpdateAndCallRecord() 
    {
        yield return new WaitForSeconds(1.1f);
        print(CalculateFrequency()[0]);
        Record();
        //StopAllCoroutines();
    }

    private float[] CalculateFrequency()
    {
        //AudioSource audioSource = GetComponent<AudioSource>();
        var samples = new float[buffersize];
        double[] samplesDoub = new double[samples.Length];
        audioSource.clip.GetData(samples, 0);


        for (int i = 0; i < samples.Length; i++)
        {
            samplesDoub[i] = samples[i];
        }

        var fft = FFT(samplesDoub);
        Array.Copy(fft, fftReal, fftReal.Length);


        double[] highestFFTValues = new double[3];
        int[] highestFFTBins = new int[3];


        var values = fftReal.Select((value, index) => new { Value = value, Index = index });
        var sortedValues = values.OrderByDescending(item => item.Value);
        var highestValues = sortedValues.Take(3);
        int j = -1;
        foreach (var item in highestValues)
        {
            j++;
            if (item.Value < .00001f) { highestFFTValues[j] = -1; continue; }
            highestFFTValues[j] = item.Value;
            highestFFTBins[j] = item.Index;
        }

        float[] frequencys = new float[3];
        for (int i = 0; i < highestFFTValues.Length; i++)
        {
            if (highestFFTValues[i] == -1) { frequencys[i] = -1; continue; }
            frequencys[i] = ((highestFFTBins[i] * sampleRate / fftReal.Length) / 2)/3;

        }
        return frequencys;
    }

    
    public static double[] FFT(double[] _data)
    {
        double[] fft = new double[_data.Length];
        System.Numerics.Complex[] fftComplex = new System.Numerics.Complex[_data.Length];

        for (int i = 0; i < _data.Length; i++)
        {
            fftComplex[i] = new System.Numerics.Complex(_data[i], 0.0);
        }
        Accord.Math.FourierTransform.FFT(fftComplex, Accord.Math.FourierTransform.Direction.Forward);
       
        for (int i = 0; i < _data.Length; i++)
        {
            fft[i] = fftComplex[i].Magnitude;
        }
        return fft;
    }


}