using UnityEngine;

public class AudioComponents : MonoBehaviour
{
    private int buffersize;

    public static AudioComponents Instance;

    private void Awake()
    {
        if (Instance != null) Destroy(this);

        Instance = this;
    }
    private void Start()
    {
        buffersize = NoteManager.Instance.DefaultBufferSize;
    }
    public float[] ExtractDataOutOfAudioClip(AudioClip _clip)
    {
        float[] samples = new float[buffersize];
        
        _clip.GetData(samples, 0);

        return samples;
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
