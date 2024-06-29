using System;
using System.Collections.Generic;
using UnityEngine;

public class Metronome : MonoBehaviour
{
    float timeElapsed = 0;
    int bpm = 0;
    float timePerBeatInSeconds;
    bool metronomeStarted = false;

    private void Start()
    {
        bpm = NoteManager.Instance.BPM;
        timePerBeatInSeconds = 60.0f / bpm;

        EventManager.StartListening("BPMChanged", SetBPM);
        EventManager.StartListening("StartedRecording", StartMetronome);
        EventManager.StartListening("StopedRecording", StopMetronome);
    }
    private void Update()
    {
        if (metronomeStarted)
        {
            timeElapsed += Time.deltaTime;

            if ((timeElapsed - Time.deltaTime) % timePerBeatInSeconds > timeElapsed % timePerBeatInSeconds || Time.deltaTime > timePerBeatInSeconds)
            {
                EventManager.TriggerEvent("NewBeat", new Dictionary<string, object>());
            }
        }
    }
    void SetBPM(Dictionary<string, object> _message)
    {
        if (_message["BPM"].ToString() == "") return;
        print("BPM:" + _message["BPM"]);
        bpm = Convert.ToInt32(_message["BPM"]);
        NoteManager.Instance.BPM = bpm;
        timePerBeatInSeconds = 60.0f / bpm;
    }
    void StartMetronome(Dictionary<string, object> _message)
    {
        metronomeStarted = true;
    }
    void StopMetronome(Dictionary<string, object> _message)
    {
        metronomeStarted = false;
    }
    private void OnDisable()
    {
        metronomeStarted = false;
    }
}