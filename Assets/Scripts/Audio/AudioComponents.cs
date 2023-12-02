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
        buffersize = NoteManager.Instance.defaultBufferSize;
    }
    public double[] ExtractDataOutOfAudioClip(AudioClip _clip)
    {
        float[] samples = new float[buffersize];
        double[] doublSamples = new double[buffersize];

        _clip.GetData(samples, 0);


        for (int i = 0; i < buffersize; i++)
        {
            doublSamples[i] = samples[i];
        }
        return doublSamples;
    }
    public double[] FFT(double[] _data)
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
