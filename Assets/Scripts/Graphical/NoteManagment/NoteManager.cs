using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NoteManager : MonoBehaviour
{
    public static NoteManager Instance;

    public Note NoteObj;
    public List<Note> PlayedNotes;
    public List<GameObject> MeasureBars;
    public int BPM = 100;
    public int NoteSpeed = 10;
    public bool PlayPaused = true;
    public int DefaultSamplerate = 11025;
    public int DefaultBufferSize = (int)Math.Pow(2, 14);
    public float HighestPossibleFrequency;
    public bool IsRecording = false;

    [SerializeField] public TMP_Text PlaybtnText;
    [SerializeField] TMP_InputField BPMInput;
    [HideInInspector] public int MaxBMP;
    private void Awake()
    {
        if (Instance != null) Destroy(this);
        BPMInput.text = BPM.ToString();
        Instance = this;
    }

    public void Play()
    {
        PlayPaused = !PlayPaused;
        PlaybtnText.text = PlayPaused ? "Play" : "Pause";
    }

    public void InstantiateNotes(Vector3[] _notePositions)
    {
        foreach (Vector3 position in _notePositions)
        {
            InstantiateNote(position);
        }
    }
    public Note InstantiateNote(Vector3 _notePosition)
    {
        Note go = Instantiate(NoteObj, _notePosition, Quaternion.identity);
        go.transform.GetChild(0).transform.GetChild(0).GetComponent<TMP_Text>().text = _notePosition.z.ToString();
        Instance.PlayedNotes.Add(go);
        return go;
    }

    public void UpdatePositionOnFretboard(Note note, Vector2 newPosition)
    {
        var newpos = new Vector3(note.transform.position.x, newPosition.x, newPosition.y);
        note.transform.position = newpos;
        note.transform.GetChild(0).transform.GetChild(0).GetComponent<TMP_Text>().text = newPosition.y.ToString();
    }
}
