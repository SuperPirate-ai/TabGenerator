using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MicrophoneInput : MonoBehaviour
{
    private AudioSource audioSource;
    private int sampleRate = 22050;
    public FFTWindow fftWindow;
    public string microphone;
    private int buffersize = (int)Mathf.Pow(2, 13);
    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        foreach(string device in Microphone.devices) 
        {
            if(microphone == null)
                microphone = device;
        }
        UpdateMicrophone();
    }
    

    private void UpdateMicrophone()
    {
        audioSource.Stop();
        audioSource.clip = Microphone.Start(microphone, false, 10, sampleRate);
        //audioSource.loop = true;
        while(Microphone.IsRecording(microphone)) { }

        byte[] audioBytes = new byte[buffersize];
        var samples = new float[audioSource.clip.samples];
        audioSource.clip.GetData(samples, 0);
        MemoryStream stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);

        foreach (var sample in samples)
        {
            writer.Write(sample);
        }

        audioBytes =  stream.ToArray();



        double[] fftReal;

        if (audioBytes.Length == 0)
            return;

        int BYTES_PER_POINT = 2;

        int pointCount = audioBytes.Length / BYTES_PER_POINT;
        fftReal = new double[pointCount / 2];

        double[] pointsY = new double[pointCount];

        for (int i = 0; i < pointCount; i++)
        {
            double point = (double)BitConverter.ToInt16(audioBytes, i * 2);
            pointsY[i] = (double)point / Math.Pow(2, 16) * 200;

        }

        var fft = AudioVisualizer.FFT(pointsY);

        Array.Copy(fft, fftReal, fftReal.Length);

        
    }


}
