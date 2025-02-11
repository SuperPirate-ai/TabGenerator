using System.Collections.Generic;
using UnityEngine;

public class Pointer : MonoBehaviour
{
    [SerializeField] int TimeSignatureNumerator;
    [SerializeField] int TimeSignatureDenominator;
    [SerializeField] Transform measureBeginnPos;
    [SerializeField] Transform measureEndPos;

    int bpm;
    float beatsPerSecond;
    float secondsPerBeat;
    float secondsPerMeasure;

    float distaceBetweenMeasures;
    float velocity;
    private void Start()
    {
        bpm = NoteManager.Instance.BPM;
        beatsPerSecond = bpm / 60f;
        secondsPerBeat = 1 / beatsPerSecond;
        secondsPerMeasure = secondsPerBeat * TimeSignatureNumerator;
        distaceBetweenMeasures = Mathf.Abs(measureBeginnPos.position.x - measureEndPos.position.x) / 2;
        velocity = distaceBetweenMeasures / secondsPerMeasure;

        EventManager.StartListening("BPMChanged", UpdateBPM);
    }
    void UpdateBPM(Dictionary<string, object> _message)
    {
        string bpmText = _message["BPM"].ToString();
        if (bpmText == string.Empty) return;
        bpm = int.Parse(bpmText);
        if (bpm == 0) return;
        beatsPerSecond = bpm / 60f;
        secondsPerBeat = 1 / beatsPerSecond;
        secondsPerMeasure = secondsPerBeat * TimeSignatureNumerator;
        distaceBetweenMeasures = Mathf.Abs(measureBeginnPos.position.x - measureEndPos.position.x) / 2;
        velocity = distaceBetweenMeasures / secondsPerMeasure;
        print("BPM" + bpm);
        print("BeatsPerSecond" + beatsPerSecond);
        print("SecondsPerBeat" + secondsPerBeat);
        print("SecondsPerMeasure" + secondsPerMeasure);
        print("DistanceBetweenMeasures" + distaceBetweenMeasures);
        print("Velocity" + velocity);
    }
    void Update()
    {
        if (NoteManager.Instance.PlayPaused)
            return;

        if (this.transform.position.x >= measureEndPos.position.x)
        {
            this.transform.position = measureBeginnPos.position;
        }
        this.transform.Translate(Vector3.right * velocity * Time.deltaTime);
        for (int i = NoteManager.Instance.PlayedNotes.Count - 1; i >= 0; i--)
        {
            GameObject note = NoteManager.Instance.PlayedNotes[i];
            if (this.transform.position.x < note.transform.position.x && this.transform.position.x + 1 > note.transform.position.x)
            {
                NoteManager.Instance.RemoveNoteFromScreen(note);
            }
        }

    }
}