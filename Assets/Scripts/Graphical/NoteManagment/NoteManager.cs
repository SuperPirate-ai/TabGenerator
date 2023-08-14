using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class NoteManager : MonoBehaviour
{
    public static NoteManager Instance { get; private set; }
    public GameObject noteObj;
    public List<Vector3> playedNotes;
    public int BPM = 100;
    public int NoteSpeed = 10;
    private void Awake()
    {
        if(Instance != null) Destroy(this); 
       
        Instance = this;
    }
    public void Play()
    {
        GameObject[] notes = GameObject.FindGameObjectsWithTag("Note");
        GameObject firstNote = null;
        foreach(GameObject note in notes)
        {
            if (firstNote == null || note.transform.position.y < firstNote.transform.position.y)
            {
                firstNote = note;
            }
        }
        foreach(GameObject note in notes)
        {
            
            note.transform.position -= new Vector3(firstNote.transform.position.x,0) ;

        }
    }
    
}
