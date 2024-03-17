using System;
using System.Collections.Generic;

using UnityEngine;

public class Metronome : MonoBehaviour
{
    public static System.Timers.Timer metronome = new System.Timers.Timer();

    int bpm = 0;
    float timeBetweenBeatsInSeconds;

    private void Start()
    {
        bpm = NoteManager.Instance.BPM;
        timeBetweenBeatsInSeconds = 60.0f / bpm;

        EventManager.StartListening("BPMChanged", SetBPM);
        EventManager.StartListening("StartedRecording", StartMetronome);
        EventManager.StartListening("StopedRecording", StopMetronome);

        metronome.Interval = timeBetweenBeatsInSeconds * 1000;
    }
    void SetBPM(Dictionary<string, object> _message)
    {
        if (_message["BPM"] == "") return;
        print(_message["BPM"]);
        bpm = System.Convert.ToInt32(_message["BPM"]);
        timeBetweenBeatsInSeconds = 60.0f / bpm;
        metronome.Interval = timeBetweenBeatsInSeconds * 1000;
    }
    void StartMetronome(Dictionary<string, object> _message)
    {
        metronome.Start();
    }
    void StopMetronome(Dictionary<string, object> _message)
    {
        metronome.Stop();
    }
    private void OnDisable()
    {
        metronome.Stop();
    }
}  