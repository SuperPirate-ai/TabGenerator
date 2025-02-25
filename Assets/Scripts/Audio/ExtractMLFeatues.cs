using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using System;

public class ExtractMLFeatues : MonoBehaviour
{
    public static ExtractMLFeatues Instance;

    private void Awake()
    {
        Instance = this;
    }
    public float AplitudeRatio(List<SNote> _overtones)
    {
        float[] amplitudesRatio = new float[_overtones.Count];
        for (int i = 0; i < _overtones.Count; i++)
        {
            amplitudesRatio[i] = _overtones[i].volume / _overtones[0].volume;
        }
        //print("AmpRatio: " + (float)Math.Round((decimal)(amplitudesRatio.Average() * .0001f), 20));
        return (float)Math.Round((decimal)(amplitudesRatio.Average() * .0001f), 20);
    }


    public float CalculteOvertoneDifference(Dictionary<int,float> _overtoneFrequencies, float _fundamentalFrequency)
    {
        List<float> overtoneDifferences = new();
        foreach(var overtone in _overtoneFrequencies)
        {
            float expectedFrequency = (float)((float)_fundamentalFrequency * (overtone.Key + 1f));
            overtoneDifferences.Add(Mathf.Abs((float)((float)overtone.Value/(float)expectedFrequency)));
        }
        if (overtoneDifferences.Count == 0) return 0;
        //print("OvertoneDiff: " + (float)Math.Round((decimal)overtoneDifferences.Average(), 20));
        return (float)Math.Round((decimal)overtoneDifferences.Average(),20);
    }

    public float CalculateAmplitudeFrequencyRatio(List<SNote> _overtones,Dictionary<int,float> _overtoneAmplitudesADDED)
    {
        float[] amplitudes = _overtones.Select(x => x.volume).ToArray();
        float[] frequencies = _overtones.Select(x => x.frequency).ToArray();

        float[] amplitudeTimesFrequencies = amplitudes.Zip(frequencies, (a, f) => (float)a * (float)f).ToArray();
        for (int i = 0; i < amplitudeTimesFrequencies.Length; i++)
        {
           // print(amplitudes[i] + " * " + frequencies[i] + " = " + amplitudeTimesFrequencies[i]);
        }
        float metric1 = 1f / (amplitudeTimesFrequencies.Sum() / amplitudes.Length);
        //print($"METRIC1: 1 / {amplitudeTimesFrequencies.Sum()} /{amplitudes.Length}  = " + metric1);
        float metric2 = _overtoneAmplitudesADDED.GetValueOrDefault(0, 0f) - _overtoneAmplitudesADDED.GetValueOrDefault(1, 1f);
        //print($"METRIC2: {_overtoneAmplitudesADDED.GetValueOrDefault(0,0f)} + {_overtoneAmplitudesADDED.GetValueOrDefault(1,0f)} = " + metric2);
        metric2 *= .0001f;
        //print($"METRIC2 smaller: {metric2}");
        //print($"GESAMT: {metric1} + {metric2} -> gerunded: {(float)Math.Round((decimal)((float)metric1 + (float)metric2), 20)}");
        return (float)Math.Round((decimal)((float)metric1 + (float)metric2),20);
    }
}

