using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;


public class AudioVisualizer : MonoBehaviour
{
    public Transform[] audioSpectrumObjects;

    [SerializeField] float heightMultiplier;
    [SerializeField] int numberOfSmaples = 1024;
    [SerializeField] float lerpTime = 1;
    [SerializeField] NotesSO notesSO;
    
    private int sampleRate = 44100;
    private int buffersize = (int)Math.Pow(2, 14);
    private double[] fftReal;
    
    private List<(int, string)> notes;
    private List<string> openWoundStringNotes;

    string LastNote = "";

    private void Awake()
    {
        fftReal = new double[numberOfSmaples];
        notes = new List<(int,string)> ();
        for (int i = 0; i < notesSO.frequnecys.Length; i++)
        {
            notes.Add((notesSO.frequnecys[i], notesSO.noteNames[i]));
        }

        openWoundStringNotes = notesSO.woundOpenStringNotes.ToList();
    }
    private void Update()
    {
        print(CalculateNote());
    }
    private float[] CalculateFrequency()
    {
        AudioSource audioSource = GetComponent<AudioSource>();
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
        var   highestValues = sortedValues.Take(3);
        int j = 0;
        foreach (var item in highestValues)
        {
            highestFFTValues[j] = item.Value;
            highestFFTBins[j] = item.Index;
            j++;
        }

        float[] frequencys = new float[3];
        for (int i = 0; i < highestFFTValues.Length; i++)
        {
            frequencys[i] = (highestFFTBins[i] * sampleRate / fftReal.Length) / 2;
           
        }

        return frequencys;
    }
    private (float,string) CalculateNote()
    {
        float[] frequencys = CalculateFrequency();
        string[] closestNotes = new string[3] { "", "", "" };
        float[] closestFreqs = new float[3] { 0f, 0f, 0f };
        float actualClosestFreq;
        string actualClosestNote;
        bool isOpenWoundString;
        //foreach (float freq in frequencys) { print(freq); }
        for (int i = 0; i < notes.Count; i++)
        {
            for (int j = 0;j<frequencys.Length;j++)
            {
                if (Mathf.Abs(notes[i].Item1 - frequencys[j]) < Mathf.Abs(closestFreqs[j] - (float)frequencys[j]))
                {
                    closestNotes[j] = notes[i].Item2;
                    closestFreqs[j] = notes[i].Item1;    
                }
            }
        }
        //foreach (int freq in closestFreqs) { print(freq); }

        for (int i = 0; i < closestFreqs.Length; i++)
        {
           // Debug.Log(closestFreqs[i]); 
            for (int a = 0; a < openWoundStringNotes.Count; a++)
            {
                if (closestNotes[i] == openWoundStringNotes[a])
                {
                    actualClosestFreq = closestFreqs[i];
                    actualClosestNote = openWoundStringNotes[a];
                    isOpenWoundString = true;
                    goto wound;
                }

            }
            
        }
        actualClosestNote = closestNotes[0];
        actualClosestFreq = closestFreqs[0];
        isOpenWoundString = false;
        wound:
        if(LastNote != actualClosestNote)
        {
            FrequenzToVisualPointConverter.Instance.OnNoteDetected(actualClosestNote,isOpenWoundString);
            LastNote = actualClosestNote;
            StartCoroutine(INewNote());
        }
        return (actualClosestFreq,actualClosestNote);
    }

    IEnumerator INewNote()
    {
        yield return new WaitForSecondsRealtime(.7f);
        LastNote = "";
        StopCoroutine(INewNote());
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
