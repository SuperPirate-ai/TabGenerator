using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MeasureBarGenerator : MonoBehaviour
{
    [SerializeField] int TimeSignitureNoteCount;
    [SerializeField] int TimeSignitureNoteValue;
    [SerializeField] GameObject MeasureBar;
    [SerializeField] GameObject Pointer;

    int beatcount = 0;
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
        GameObject go = Instantiate(MeasureBar.gameObject, Pointer.transform.position, Quaternion.identity);
        NoteManager.Instance.MeasureBars.Add(go);
    }
    
    
}
