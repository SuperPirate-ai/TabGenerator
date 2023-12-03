using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NoteManager : MonoBehaviour
{
    public static NoteManager Instance { get; private set; }

    public GameObject noteObj;
    public List<GameObject> playedNotes;
    public int BPM = 100;
    public int NoteSpeed = 10;
    public bool PlayPaused = true;
    public int DefaultSamplerate = 11025;
    public int DefaultBufferSize = (int)Math.Pow(2, 14);

    [HideInInspector] public int MaxBMP;
    private void Awake()
    {
        if (Instance != null) Destroy(this);

        Instance = this;
    }
    public void Play(TMP_Text btnText)
    {
        PlayPaused = !PlayPaused;
        btnText.text = PlayPaused ? "Play" : "Pause";
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
        GameObject go = Instantiate(noteObj, _notePosition, Quaternion.identity);
        go.transform.GetChild(0).transform.GetChild(0).GetComponent<TMP_Text>().text = _notePosition.z.ToString();
        Instance.playedNotes.Add(go);
    }
}
