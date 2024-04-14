using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MeasureBarGenerator : MonoBehaviour
{
    int beatcount = 0;
    [SerializeField] int TimeSignitureNoteCount;
    [SerializeField] int TimeSignitureNoteValue;
    [SerializeField] GameObject MeasureBar;
    [SerializeField] GameObject Pointer;

    bool instatiateMeasureBar = false;
    
    private void Start()
    {
        EventManager.StartListening("NewBeat", OnNewBeat);
    }
    private void FixedUpdate()
    {
        if(instatiateMeasureBar)
        {
            CreateNewMeasureBar();
            instatiateMeasureBar = false;
        }
    }
    void OnNewBeat(Dictionary<string,object> _message)
    {
        beatcount++;
        if(beatcount % TimeSignitureNoteCount == 0) 
        {
            print("New Measure");
            instatiateMeasureBar = true;
        }
    }
    void CreateNewMeasureBar()
    {
        Debug.Log("Instantiating...");

        GameObject go = Instantiate(MeasureBar.gameObject, Pointer.transform.position, Quaternion.identity);
        NoteManager.Instance.MeasureBars.Add(go);
        print("Distance from last measure bar: " + (NoteManager.Instance.MeasureBars[NoteManager.Instance.MeasureBars.Count - 2].gameObject.transform.position.x - go.transform.position.x));
    }
    
    
}
