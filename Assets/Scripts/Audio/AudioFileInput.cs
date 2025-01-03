using UnityEngine;

public class AudioFileInput : MonoBehaviour
{
    [SerializeField] AudioClip audioClip;
    [SerializeField] AudioAnalyzer analyser;

    public void StartAnalysingBtn()
    {
        float[] samples = AudioComponents.Instance.ExtractDataOutOfAudioClip(audioClip, 0);
        analyser.Analyze(samples);
    }
    public void TestPickStrokeDetectionbtn()
    {
        float[] samples = { 1, 1.5f, 1 };
        // print($"Pick stroke detected: {AudioComponents.Instance.DetectPickStroke(samples, 0)}");
    }
}
