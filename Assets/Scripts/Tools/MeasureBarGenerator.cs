using System;
using System.Collections;
using System.Collections.Generic;
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
        Metronome.metronome.Elapsed += OnNewBeat;
    }
    private void Update()
    {
        if(instatiateMeasureBar)
        {
            InstanciateNewMeasureBar();
            instatiateMeasureBar = false;
        }
    }
    void OnNewBeat(object _sender, EventArgs _e)
    {
        beatcount++;
        //print(beatcount);
        if(beatcount % TimeSignitureNoteCount == 0) 
        {
            print("New Measure");
            instatiateMeasureBar = true;
        }
    }
    void InstanciateNewMeasureBar()
    {
        Debug.Log("Instantiating...");
        try
        {
            GameObject go = Instantiate(MeasureBar.gameObject, Pointer.transform.position, Quaternion.identity);
            NoteManager.Instance.MeasureBars.Add(go);
        }
        catch(Exception e) 
        {
            print(e);
        }
       
    }
}
