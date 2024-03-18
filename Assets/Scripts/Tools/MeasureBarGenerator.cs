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
        EventManager.StartListening("BPMChanged", OnBPMChange);
    }
    private void Update()
    {
        if(instatiateMeasureBar)
        {
            CreateNewMeasureBar();
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
    void CreateNewMeasureBar()
    {
        Debug.Log("Instantiating...");

        GameObject go = Instantiate(MeasureBar.gameObject, Pointer.transform.position, Quaternion.identity);
        NoteManager.Instance.MeasureBars.Add(go);
    }
    void InstanciateMeasureBar(GameObject _measureBar, Vector3 _position)
    {
        GameObject go = Instantiate(MeasureBar.gameObject, Pointer.transform.position, Quaternion.identity);
        NoteManager.Instance.MeasureBars.Add(go);
    }
    void OnBPMChange(Dictionary<string, object> _message)
    {
        List<GameObject> measurebars = NoteManager.Instance.MeasureBars;
        NoteManager.Instance.MeasureBars.Clear();
        int oldBPM = NoteManager.Instance.BPM;
        int newBPM = Convert.ToInt32(_message["BPM"]);
        NoteManager.Instance.BPM = Convert.ToInt32(_message["BPM"]);
                
        float distanceBewtweenOLDBars = (Mathf.Abs(measurebars[0].transform.position.x) - Mathf.Abs(measurebars[1].transform.position.x));
        float timeBetweenOLDBars = 60 / oldBPM;

        float timeBetweenBars = 60 / newBPM;

        var value = distanceBewtweenOLDBars / timeBetweenOLDBars;

        float distanceBetweenNEWBars = value * timeBetweenBars;

        foreach (var item in measurebars)
        {
            try
            {
                Destroy(item.gameObject);
            }
            catch(Exception e)
            {
                print(e);
            }
        }
        
    }
}
