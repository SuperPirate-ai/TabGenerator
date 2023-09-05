using UnityEngine;

public class AudioFilter : MonoBehaviour
{
    public static AudioFilter Instance;
    private void Awake()
    {
        if (Instance != null) Destroy(this);
        Instance = this;
    }
    public double[] HighPassFilter(float _cutofFrequency, double[] _fftSamples)
    {
        double[] filterdFFT = new double[_fftSamples.Length];
        int cutofBin = (int)(_cutofFrequency * _fftSamples.Length * 2 / NoteManager.Instance.defaultSamplerate);

        for (int i = cutofBin; i >= 0; i--)
        {
            _fftSamples[i] = 0;
        }
        filterdFFT = _fftSamples;
        return filterdFFT;

    }

    
}
