using System.Linq;
using UnityEngine;

public class AudioComponents : MonoBehaviour
{
    private int buffersize;
    public static AudioComponents Instance;
    private float lastSubsampleLoudnessOfPreviousBuffer = Mathf.Infinity;

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
    public bool DetectPickStroke(float[] _samples)
    {
        int subsamples = 3; // => needs to be a power of 2 || 128 -> 2^7
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

            if (subsampleLoudnesses[i] * 1.5f < subsampleLoudnesses[i + 1])
            {
                stroke = true;
                break;
            }
        }
        if (lastSubsampleLoudnessOfPreviousBuffer * 1.5f < subsampleLoudnesses[0])
        {
            stroke = true;

        }

        lastSubsampleLoudnessOfPreviousBuffer = subsampleLoudnesses.Last();
        return stroke;
    }

    public float DetectOctaveInterference(float _frequnecy, float[] _fftReal, int _freqBin)
    {
        float lowerOctaveFrequency = _frequnecy / 2;
        int lowerOctaveBin = (int)((float)lowerOctaveFrequency / (float)NoteManager.Instance.DefaultSamplerate * (float)_fftReal.Length);
        
        float lowerOctaveAmplitude = _fftReal[lowerOctaveBin];
        float freqAmplitudedReduced = _fftReal[_freqBin] * 0.75f;

        if (freqAmplitudedReduced < lowerOctaveAmplitude)
        {
            return lowerOctaveFrequency;
        }
        return _frequnecy;

    }
    public float[] FFT(float[] _data)
    {
        float[] fft = new float[_data.Length];
        System.Numerics.Complex[] fftComplex = new System.Numerics.Complex[_data.Length];

        for (int i = 0; i < _data.Length; i++)
        {
            fftComplex[i] = new System.Numerics.Complex(_data[i], 0.0);
        }

        Accord.Math.FourierTransform.FFT(fftComplex, Accord.Math.FourierTransform.Direction.Forward);

        for (int i = 0; i < _data.Length; i++)
        {
            fft[i] = (float)fftComplex[i].Magnitude;
        }

        return fft;
    }
}
