using System;
using System.Collections.Generic;

using UnityEngine;

public class Metronome : MonoBehaviour
{
    //public static System.Timers.Timer metronome = new System.Timers.Timer();
    
    float timeElapsed = 0;
    int bpm = 0;
    float timePerBeatInSeconds;
    float remainingTimeTillNextBeat = 0;
    bool metronomeStarted = false;
    DateTime startTime;

    private void Start()
    {
        bpm = NoteManager.Instance.BPM;
        timePerBeatInSeconds = 60.0f / bpm;

        EventManager.StartListening("BPMChanged", SetBPM);
        EventManager.StartListening("StartedRecording", StartMetronome);
        EventManager.StartListening("StopedRecording", StopMetronome);

        //metronome.Interval = timePerBeatInSeconds * 1000;
    }
    private void Update()
    {
        if(metronomeStarted)
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
        if (_message["BPM"] == "") return;
        print("BPM:" + _message["BPM"]);
        bpm = System.Convert.ToInt32(_message["BPM"]);
        NoteManager.Instance.BPM = bpm;
        timePerBeatInSeconds = 60.0f / bpm;
       // metronome.Interval = timePerBeatInSeconds * 1000;
    }
    void StartMetronome(Dictionary<string, object> _message)
    {
        metronomeStarted = true;
        startTime = DateTime.Now;
        //metronome.Start();
    }
    void StopMetronome(Dictionary<string, object> _message)
    {
        metronomeStarted = false;
        //metronome.Stop();
        SetRemainingTimeTillNextBeat();
    }
    void SetRemainingTimeTillNextBeat()
    {
        remainingTimeTillNextBeat = timePerBeatInSeconds - (float)((DateTime.Now - startTime).Seconds % timePerBeatInSeconds);
        print("remainingTimeTillNextBeat: " + remainingTimeTillNextBeat);
    }
    private void OnDisable()
    {
        metronomeStarted = false;
        //metronome.Stop();
    }
}  