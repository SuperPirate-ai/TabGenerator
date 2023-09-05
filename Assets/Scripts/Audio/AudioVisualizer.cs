using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;


public class AudioVisualizer : MonoBehaviour
{
    [SerializeField] int numberOfSmaples = 8192;
    [SerializeField] NotesSO notesSO;
    
    private int sampleRate;
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
        sampleRate = NoteManager.Instance.defaultSamplerate;
    }
   
    private float[] CalculateFrequency(AudioClip _clip)
    {
        //AudioSource audioSource = GetComponent<AudioSource>();
        var samples = new float[buffersize];
        double[] samplesDoub = new double[samples.Length];
        _clip.GetData(samples, 0);


        for (int i = 0; i < samples.Length; i++)
        {
            samplesDoub[i] = samples[i];
        }

        var fft = FFT(samplesDoub);
        Array.Copy(fft, fftReal, fftReal.Length);

        //taking higpassFilter
        fftReal = AudioFilter.Instance.HighPassFilter(75, fftReal);

        double[] highestFFTValues = new double[3];
        int[] highestFFTBins = new int[3];

        var values = fftReal.Select((value, index) => new { Value = value, Index = index });
        var sortedValues = values.OrderByDescending(item => item.Value);
        var   highestValues = sortedValues.Take(3);
        int j = -1;
        foreach (var item in highestValues)
        {
            j++;
            if (item.Value < .001f) { highestFFTValues[j] = -1; continue; }
            highestFFTValues[j] = item.Value;
            highestFFTBins[j] = item.Index;
        }

        float[] frequencys = new float[3];
        for (int i = 0; i < highestFFTValues.Length; i++)
        {
            if (highestFFTValues[i] == -1) { frequencys[i] = -1; continue; }
            frequencys[i] = (highestFFTBins[i] * sampleRate / fftReal.Length) / 2;
           
        }
        return frequencys;
    }
    public (float,string) CalculateNote(AudioClip _clip)
    {
        float[] frequencys = CalculateFrequency(_clip);
        string[] closestNotes = new string[3] { "", "", "" };
        float[] closestFreqs = new float[3] { 0f, 0f, 0f };
        float actualClosestFreq;
        string actualClosestNote;
        bool isOpenWoundString;

        for (int i = 0; i < notes.Count; i++)
        {
            for (int j = 0;j<frequencys.Length;j++)
            {
                if (frequencys[j] == -1) continue;
                if (Mathf.Abs(notes[i].Item1 - frequencys[j]) < Mathf.Abs(closestFreqs[j] - (float)frequencys[j]) && Mathf.Abs(notes[i].Item1 - frequencys[j]) < 40f)
                {
                    closestNotes[j] = notes[i].Item2;
                    closestFreqs[j] = notes[i].Item1;    
                }
            }
        }

        if (closestFreqs[0] + closestFreqs[1] + closestFreqs[2] == 0f) return (-1,"None");
        for (int i = 0; i < closestFreqs.Length; i++)
        {
           
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
        if(actualClosestFreq == 0)return (-1,"NONE");
        if(LastNote != actualClosestNote)
        {
            Debug.Log("Freq: " + actualClosestFreq + "Note: " + actualClosestNote);


            string[] notePos = NoteToVisualPointsConverter.Instance.GetNotePositions(actualClosestNote);
            NoteManager.Instance.InstantiateNotes(notePos,isOpenWoundString);
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
