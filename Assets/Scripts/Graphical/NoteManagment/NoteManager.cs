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
    public int defaultSamplerate = 44100 ;
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
   
    public void InstantiateNotes(Vector3[] _notePositions, bool _isOpenWoundString)
    {
        
        foreach (Vector3 position in _notePositions)
        {

            if (_isOpenWoundString && position.x != 0) continue;

            GameObject go = Instantiate(noteObj,position, Quaternion.identity);
            go.transform.GetChild(0).transform.GetChild(0).GetComponent<TMP_Text>().text = position.z.ToString();
            Instance.playedNotes.Add(go);
            if (_isOpenWoundString)
            {
                break;
            }
            if (_notePositions.Length > 1)
            {
                go.GetComponent<Renderer>().material.color = new Color(202, 1, 0);//orange
            }
        }
    }
   
}
