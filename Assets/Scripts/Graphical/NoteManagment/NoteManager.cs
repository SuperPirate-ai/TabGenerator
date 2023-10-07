using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class NoteManager : MonoBehaviour
{
    public static NoteManager Instance { get; private set; }
    public GameObject noteObj;
    public List<GameObject> playedNotes;
    public int BPM = 100;
    public int NoteSpeed = 10;
    public bool PlayPaused = true;
    public int defaultSamplerate = 44100;
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

    public void InstantiateNotes(Vector3[] _notePositions, bool _isOpenWoundString)
    {

        foreach (Vector3 position in _notePositions)
        {
           int code = InstantiateNotes(position, _isOpenWoundString);
            if (code == 1) break;
        }
    }
    public int InstantiateNotes(Vector3 _notePositions, bool _isOpenWoundString)
    {
        if (_isOpenWoundString && _notePositions.x != 0) return 0;

        GameObject go = Instantiate(noteObj, _notePositions, Quaternion.identity);
        go.transform.GetChild(0).transform.GetChild(0).GetComponent<TMP_Text>().text = _notePositions.z.ToString();
        Instance.playedNotes.Add(go);
        
        if (_isOpenWoundString)
        {
            return 1;
        }
        return 0;
        
    }

}