using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Note : MonoBehaviour
{
    NoteManager manager;
    int bpm;
    int speed;
    private void OnEnable()
    {
        manager = NoteManager.Instance;
        bpm = manager.BPM;
        speed = manager.NoteSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 velocity = new Vector3(-speed * Time.deltaTime, 0);
        this.transform.Translate(velocity);
    }
}
