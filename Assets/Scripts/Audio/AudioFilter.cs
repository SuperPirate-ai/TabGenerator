using UnityEngine;

public class AudioFilter : MonoBehaviour
{
    float lowestFrequency;
    float highestFrequency;
    double[] fftSamples;
    public AudioFilter(float _lowestFrequency, float _highestFrequency, double[] _fftSamples)
    {
        lowestFrequency = _lowestFrequency;
        highestFrequency = _highestFrequency;
        fftSamples = _fftSamples;
    }
    public double[] HighAndLowPassFilter()
    {
        double[] filteredFFT = new double[fftSamples.Length];
        double[] highpass = HighPassFilter(lowestFrequency, fftSamples);
        double[] lowpass = LowPassFilter(highestFrequency, fftSamples);
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
    double[] AllPassFilter(float _cutofFrequency, double[] _fftSamples)
    {
        float fb = _cutofFrequency;
        float fs = NoteManager.Instance.DefaultSamplerate;

        float b = Mathf.Tan((Mathf.PI * fb) / fs);
        float a = b - 1 / b + 1;

        double[] output = new double[_fftSamples.Length];
        for (int i = 0; i < _fftSamples.Length; i++)
        {
            output[i] = (a + Mathf.Pow(i, -1)) / (1 + (a * Mathf.Pow(i, -1)));
        }
        return output;
    }
    public double[] HighPassFilter(float _cutofFrequency, double[] _fftSamples)
    {
        int length = _fftSamples.Length;
        double[] filterdFFT = new double[length];
        double[] allpass = AllPassFilter(_cutofFrequency, _fftSamples);

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
    public double[] LowPassFilter(float _cutofFrequency, double[] _fftSamples)
    {
        double[] filterdFFT = _fftSamples;
        int cutofBin = (int)(_cutofFrequency * _fftSamples.Length * 2 / NoteManager.Instance.DefaultSamplerate);
        for (int i = cutofBin; i < _fftSamples.Length; i++)
        {
            filterdFFT[i] = 0;
        }
        return filterdFFT;
    }


}
