using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class NoteManager : MonoBehaviour
{
    public static NoteManager Instance { get; private set; }

    public List<Vector3> playedNotes;
    public int BPM = 100;
    public int NoteSpeed = 10;
    private void Awake()
    {
        if(Instance != null) Destroy(this); 
       
        Instance = this;
    }
    
}
