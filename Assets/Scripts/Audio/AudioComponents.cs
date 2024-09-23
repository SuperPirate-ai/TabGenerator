using System.Linq;
using UnityEngine;
using System.Numerics;
using System.Collections.Generic;

public class AudioComponents : MonoBehaviour
{
    private int buffersize;
    public static AudioComponents Instance;
    private float lastSubsampleLoudnessOfPreviousBuffer = Mathf.Infinity;
    public int earlyReturnCounter = 0;

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
            lastSubsampleLoudnessOfPreviousBuffer = Mathf.Infinity;
        }
    }
    public float[] ExtractDataOutOfAudioClip(AudioClip _clip, int _positionInClip)
    {
        float[] samples = new float[buffersize];

        _clip.GetData(samples, _positionInClip);

        return samples;
    }
    int a = 0;
    public bool DetectPickStroke(float[] _samples, float _frequency)
    {
        if(a == 0)
        {
            HilbertTransform(_samples);
        }
        if(a <=10)
        {
             a++;
        }
        if(a == 10)
        {
            HilbertTransform(_samples);
        }
        earlyReturnCounter = 0;
        int subsamples = /*_frequency > 150 ?*/ 64 /*: 16*/; // => needs to be a power of 2 || 128 -> 2^7
        int subsampleSize = _samples.Length / subsamples;
        float[] subsampleLoudnesses = new float[subsamples];


        for (int i = 0; i < subsampleLoudnesses.Length; i++)
        {
            for (int j = i * subsampleSize; j < i * subsampleSize + subsampleSize; j++)
            {
                if (Mathf.Abs(_samples[j]) > subsampleLoudnesses[i])
                {
                    subsampleLoudnesses[i] = Mathf.Abs(_samples[j]);
                }
            }
        }

        bool stroke = false;

        for (int i = 0; i < subsampleLoudnesses.Length - 1; i++)
        {

            if (subsampleLoudnesses[i] * 2f < subsampleLoudnesses[i + 1])
            {
                stroke = true;
                break;
            }
        }
        if (lastSubsampleLoudnessOfPreviousBuffer * 2f < subsampleLoudnesses[0])
        {
            stroke = true;

        }

        lastSubsampleLoudnessOfPreviousBuffer = subsampleLoudnesses.Last();
        return stroke;
    }
    private float[] HilbertTransform(float[] _samples)
    {
        Complex[] fft = new Complex[_samples.Length];

        for (int i = 0; i < _samples.Length; i++)
        {
            fft[i] = new Complex(_samples[i], 0.0);
        }

        Accord.Math.FourierTransform.FFT(fft,Accord.Math.FourierTransform.Direction.Forward);

        for (int i = 1; i < (_samples.Length + 1) / 2; i++)
        {
            fft[i] *= 2;
        }
        for (int i = (_samples.Length + 1) / 2; i < _samples.Length; i++)
        {
            fft[i] = Complex.Zero;
        }
        Accord.Math.FourierTransform.FFT(fft, Accord.Math.FourierTransform.Direction.Backward);

        float[] envelope = new float[_samples.Length];

        for (int i = 0; i < _samples.Length; i++)
        {
            envelope[i] = (float)fft[i].Magnitude;
        }

        Dictionary<float, float> envelopeValues = new Dictionary<float, float>();
        for (int i = 0; i < envelope.Length; i++)
        {
            envelopeValues.Add(i, envelope[i]);
        }
        //GraphPlotter.Instance.PlotGraph(envelopeValues);

        Dictionary<float,float> normalValues = new Dictionary<float, float>();
        for (int i = 0; i < _samples.Length; i++)
        {
            normalValues.Add(i, _samples[i]);
        }
        //GraphPlotter.Instance.PlotGraph(normalValues);

        return envelope;
    }
    public void ListenForEarlyReturn()
    {
        if (earlyReturnCounter > 0)
        {
            lastSubsampleLoudnessOfPreviousBuffer = 0;
        }
        earlyReturnCounter++;
    }
    public float DetectOctaveInterference(float _frequency, float[] _fftReal, int _freqBin)
    {
        float lowerOctaveFrequency = _frequency / 2;
        int lowerOctaveBin = (int)((float)lowerOctaveFrequency / (float)NoteManager.Instance.DefaultSamplerate * (float)_fftReal.Length);

        float lowerOctaveAmplitude = _fftReal[lowerOctaveBin];
        float freqAmplitudedReduced = _fftReal[_freqBin] * 0.60f;

        if (freqAmplitudedReduced < lowerOctaveAmplitude)
        {
            return lowerOctaveFrequency;
        }
        return _frequency;

    }
    public float[] FFT(float[] _data)
    {
        float[] fft = new float[_data.Length];
       Complex[] fftComplex = new Complex[_data.Length];

        for (int i = 0; i < _data.Length; i++)
        {
            fftComplex[i] = new Complex(_data[i], 0.0);
        }

        Accord.Math.FourierTransform.FFT(fftComplex, Accord.Math.FourierTransform.Direction.Forward);

        for (int i = 0; i < _data.Length; i++)
        {
            fft[i] = (float)fftComplex[i].Magnitude;
        }

        return fft;
    }
}
