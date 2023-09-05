using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Note : MonoBehaviour
{
    private NoteManager manager;
    private int bpm;
    private int speed;
    private void OnEnable()
    {
        manager = NoteManager.Instance;
        bpm = manager.BPM;
        speed = manager.NoteSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        if(!manager.playedNotes.Contains(this.gameObject)) Destroy(this.gameObject);

        Move();
    }
    void Move()
    {
        if (NoteManager.Instance.PlayPaused) return;

        Vector3 velocity = new Vector3(-speed * Time.deltaTime, 0);
        this.transform.Translate(velocity);
    }
}
