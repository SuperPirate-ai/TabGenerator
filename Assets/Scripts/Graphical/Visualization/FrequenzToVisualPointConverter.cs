using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class FrequenzToVisualPointConverter : MonoBehaviour
{
    public static FrequenzToVisualPointConverter Instance { get; private set; }

    [SerializeField] NotesSO notes;
    [SerializeField] StringReferenzSO guitarStringRefernez;
    [SerializeField] GameObject noteObj;


    private void OnEnable()
    {
        if(Instance != null) Destroy(this);
        Instance = this;
    }
    public void OnNoteDetected(string _note,bool _isOpenWoundString)
    {
        int indexOfNote = -1;
        
        for (int i = 0; i < notes.noteNames.Length; i++)
        {
            if (notes.noteNames[i] == _note)
            {
                indexOfNote = i;
                break;
            }
        }

        if (indexOfNote == -1) { Debug.Log($"No Note found with the name{_note}."); return; }

        string[] notePositions = guitarStringRefernez.NotePositions[indexOfNote].Split(';');

        foreach (var pos in notePositions)
        {

            string[] positions = pos.Split(',');
            if(positions.Length > 2) { Debug.Log("ERROR!! More than two position Vakues."); return; }

            if (_isOpenWoundString && System.Convert.ToInt16(positions[1]) != 0) continue;

            int  y = System.Convert.ToInt16(positions[0]);
            GameObject go = Instantiate(noteObj, new Vector3(0, -y), Quaternion.identity);
            go.transform.GetChild(0).transform.GetChild(0).GetComponent<TMP_Text>().text = positions[1];

            if(_isOpenWoundString)
            {
                break;
            }
            if(notePositions.Length > 1)
            {
                go.GetComponent<Renderer>().material.color = new Color(202,1,0);//orange
            }
        }
       
       
    }
}
