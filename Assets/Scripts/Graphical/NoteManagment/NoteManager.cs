using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NoteManager : MonoBehaviour
{
    public static NoteManager Instance { get; private set; }

    public GameObject NoteObj;
    public List<GameObject> PlayedNotes;
    public List<GameObject> MeasureBars;
    public int BPM = 100;
    public int NoteSpeed = 10;
    public bool PlayPaused = true;
    [HideInInspector] public int DefaultSamplerate = 96000;
    [HideInInspector] public int DefaultBufferSize = (int)Math.Pow(2, 12);
    public float HighestPossibleFrequency;
    public bool IsRecording = false;

    [SerializeField] TMP_Text PlaybtnText;
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
    public void InstantiateNote(Vector3 _notePosition)
    {
        int fret = (int)_notePosition.z;
        string fretString = fret.ToString();
        _notePosition.z = 0;
        GameObject go = Instantiate(NoteObj, _notePosition, Quaternion.identity);
        _notePosition.z = fret;
        go.transform.GetChild(0).transform.GetChild(0).GetComponent<TMP_Text>().text = fretString;
        Instance.PlayedNotes.Add(go);
    }
    public void RemoveNoteFromScreen(GameObject _note)
    {
        Destroy(_note);
        PlayedNotes.Remove(_note);
    }
}
