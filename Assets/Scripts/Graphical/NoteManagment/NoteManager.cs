using System.Collections;
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
    private void Awake()
    {
        if(Instance != null) Destroy(this); 
       
        Instance = this;
    }
    public void Play(TMP_Text btnText)
    {
        PlayPaused = !PlayPaused;
        btnText.text = PlayPaused ? "Play" : "Pause";
    }
    
}
