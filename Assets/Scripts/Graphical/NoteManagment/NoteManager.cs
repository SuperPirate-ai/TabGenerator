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
    public void InstantiateNotes(string[] _notePositions, bool _isOpenWoundString)
    {
        foreach (var pos in _notePositions)
        {

            string[] positions = pos.Split(',');
            //print(pos  + "||" + positions[0]);
            if (positions.Length > 3) { Debug.Log("ERROR!! More than tree position Vakues."); return; }

            if (_isOpenWoundString && System.Convert.ToInt16(positions[1]) != 0) continue;

            float y = (float)System.Convert.ToDouble(positions[0]);
            int z = System.Convert.ToInt32(positions[1]);
            float x = positions.Length > 2 ? (float)System.Convert.ToDouble(positions[2]) : 0f;
            GameObject go = Instantiate(noteObj, new Vector3(x, y, z), Quaternion.identity);
            go.transform.GetChild(0).transform.GetChild(0).GetComponent<TMP_Text>().text = positions[1];
            NoteManager.Instance.playedNotes.Add(go);
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
