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
    public int DefaultSamplerate = 11025;
    public int DefaultBufferSize = (int)Math.Pow(2, 14);
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
        GameObject go = Instantiate(NoteObj, _notePosition, Quaternion.identity);
        go.transform.GetChild(0).transform.GetChild(0).GetComponent<TMP_Text>().text = _notePosition.z.ToString();
        Instance.PlayedNotes.Add(go);
    }
}
