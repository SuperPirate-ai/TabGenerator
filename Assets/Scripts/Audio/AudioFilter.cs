using UnityEngine;

public class AudioFilter : MonoBehaviour
{
    float lowestFrequency;
    float highestFrequency;
    float[] fftSamples;
    public AudioFilter(float _lowestFrequency, float _highestFrequency, float[] _fftSamples)
    {
        lowestFrequency = _lowestFrequency;
        highestFrequency = _highestFrequency;
        fftSamples = _fftSamples;
    }
    public float[] HighAndLowPassFilter()
    {
        float[] filteredFFT = new float[fftSamples.Length];
        float[] highpass = HighPassFilter(lowestFrequency, fftSamples);
        float[] lowpass = LowPassFilter(highestFrequency, fftSamples);
        int cutofBinLowPass = (int)(highestFrequency * fftSamples.Length * 2 / NoteManager.Instance.DefaultSamplerate);
        int cutofBinHighPass = (int)(lowestFrequency * fftSamples.Length * 2 / NoteManager.Instance.DefaultSamplerate);

        for (int i = 0; i < filteredFFT.Length; i++)
        {
            if (i <= cutofBinHighPass)
            {
                filteredFFT[i] = highpass[i];
            }
            else if (i >= cutofBinLowPass)
            {
                filteredFFT[i] = lowpass[i];
            }
            else
            {
                i = cutofBinLowPass;
            }
        }
        return filteredFFT;
    }
    float[] AllPassFilter(float _cutofFrequency, float[] _fftSamples)
    {
        float fb = _cutofFrequency;
        float fs = NoteManager.Instance.DefaultSamplerate;

        float b = Mathf.Tan((Mathf.PI * fb) / fs);
        float a = b - 1 / b + 1;

        float[] output = new float[_fftSamples.Length];
        for (int i = 0; i < _fftSamples.Length; i++)
        {
            output[i] = (a + Mathf.Pow(i, -1)) / (1 + (a * Mathf.Pow(i, -1)));
        }
        return output;
    }
    public float[] HighPassFilter(float _cutofFrequency, float[] _fftSamples)
    {
        int length = _fftSamples.Length;
        float[] filterdFFT = new float[length];
        float[] allpass = AllPassFilter(_cutofFrequency, _fftSamples);

        for (int i = 0; i < length; i++)
        {
            filterdFFT[i] = _fftSamples[i] + allpass[i];
        }
        //int cutofBin = (int)(_cutofFrequency * _fftSamples.Length * 2 / NoteManager.Instance.defaultSamplerate);

        //for (int i = cutofBin; i >= 0; i--)
        //{
        //    filterdFFT[i] = 0;
        //}
        



        return filterdFFT;

    }
    public float[] LowPassFilter(float _cutofFrequency, float[] _fftSamples)
    {
        float[] filterdFFT = _fftSamples;
        int cutofBin = (int)(_cutofFrequency * _fftSamples.Length * 2 / NoteManager.Instance.DefaultSamplerate);
        for (int i = cutofBin; i < _fftSamples.Length; i++)
        {
            filterdFFT[i] = 0;
        }
        return filterdFFT;
    }


}
