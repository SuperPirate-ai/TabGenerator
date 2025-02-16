using Accord.Statistics;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class ExtractMLFeatues : MonoBehaviour
{
    public static ExtractMLFeatues Instance;

    private void Awake()
    {
        Instance = this;
    }



    public float CalculteOvertoneDifference(float[] _fft, float _fundamentalFrequency)
    {
        float[] multiplesFundFreq = Enumerable.Range(1, 5).Select(i => _fundamentalFrequency * i).ToArray();
        float freqRangePercentage = .1f;
        float[] differences = new float[multiplesFundFreq.Length];

        for (int i = 0; i < multiplesFundFreq.Length; i++)
        {
            float range = multiplesFundFreq[i] * freqRangePercentage;
            int indexRange = (int)(range * NoteManager.Instance.DefaultBufferSize / NoteManager.Instance.DefaultSamplerate);
            int indexOfMultiple = (int)(multiplesFundFreq[i] * NoteManager.Instance.DefaultBufferSize / NoteManager.Instance.DefaultSamplerate);
            float maxAmplitude = 0f;
            int maxIndex = -1;
            float threshold = _fft.Max() * .001f;
            for (int j = -indexRange; j < indexRange; j++)
            {
                int currentIndex = indexOfMultiple + j;
                if (currentIndex >= 0 && currentIndex < _fft.Length && _fft[currentIndex] > maxAmplitude && _fft[currentIndex] > threshold)
                {
                    maxAmplitude = _fft[currentIndex];
                    maxIndex = currentIndex;
                }
            }

            float overtoneFrequency = (float)maxIndex / NoteManager.Instance.DefaultBufferSize * NoteManager.Instance.DefaultSamplerate;
            differences[i] = Mathf.Abs(overtoneFrequency - multiplesFundFreq[i]);
        }

        return differences.Average();
    }

    public float CalculateAmplitudeFrequencyRatio(List<SNote> overtones)
    {
        float[] amplitudes = overtones.Select(x => x.volume).ToArray();
        float[] frequencies = overtones.Select(x => x.frequency).ToArray();

        float ratio = 0;

      

        for (int i = 0; i < amplitudes.Length; i++)
        {
            ratio += amplitudes[i] * frequencies[i];
        }
        float avgRatio = 1/ratio / amplitudes.Length;//metric_1



        int ampLength = amplitudes.Length;
        float metric_2 = (0 <  ampLength? amplitudes[0]: 0) - (1< ampLength? amplitudes[1]: 1);

        metric_2 *= .0001f;

        return 0;




    }
}

