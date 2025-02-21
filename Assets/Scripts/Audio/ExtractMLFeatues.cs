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
        return (float)Math.Round((decimal)(amplitudesRatio.Average() * .0001f), 20);
    }


    public float CalculteOvertoneDifference(Dictionary<int,float> _overtoneFrequencies, float _fundamentalFrequency)
    {
        List<float> overtoneDifferences = new();
        foreach(var overtone in _overtoneFrequencies)
        {
            float expectedFrequency = (float)((float)_fundamentalFrequency * (overtone.Key + 1));
            overtoneDifferences.Add(Mathf.Abs((float)((float)overtone.Value/(float)expectedFrequency)));
        }
        if (overtoneDifferences.Count == 0) return 0;
        return (float)Math.Round((decimal)overtoneDifferences.Average(),20);
    }

    public float CalculateAmplitudeFrequencyRatio(List<SNote> _overtones,Dictionary<int,float> _overtoneAmplitudesADDED)
    {
        float[] amplitudes = _overtones.Select(x => x.volume).ToArray();
        float[] frequencies = _overtones.Select(x => x.frequency).ToArray();

        float ratio = 0;
        float[] amplitudeTimesFrequencies = amplitudes.Zip(frequencies, (a, f) => a * f).ToArray();
        float metric1 = 1 / (amplitudeTimesFrequencies.Sum() / amplitudes.Length);



        float metric2 = _overtoneAmplitudesADDED.GetValueOrDefault(0, 0f) - _overtoneAmplitudesADDED.GetValueOrDefault(1, 1f);

        metric2 *= .0001f;

        return (float)Math.Round((decimal)((float)metric1 + (float)metric2),20);
    }
}
//// Compute metric 1
//List<float> amplitudeTimesFrequencies = amplitudePeaks.Zip(frequencyPeaks, (a, f) => a * f).ToList();
//float metric1 = 1 / (amplitudeTimesFrequencies.Sum() / amplitudePeaks.Count);

//// Compute metric 2
//float metric2 = overtoneAmplitudesADDED.GetValueOrDefault(0, 0f) - overtoneAmplitudesADDED.GetValueOrDefault(1, 1f);
//metric2 *= 0.0001f;
