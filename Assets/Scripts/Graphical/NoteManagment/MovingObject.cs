using Codice.CM.Common;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MovingObject : MonoBehaviour
{
    private int speed;
    [HideInInspector]public NoteManager manager;

    private void Start()
    {
        speed = manager.NoteSpeed;
    }
    public void OnEnable()
    {
        manager = NoteManager.Instance;
    }
    public void Update()
    {
        Move();

        // change the speed according to the BPM set in NoteManager.Instace.BPM and set 100 BPM as a speed of 10
        speed = (int)(manager.NoteSpeed * (manager.BPM / 100f));

    }
    public void Move()
    {

        if (manager.PlayPaused) return;

        Vector3 velocity = new Vector3(-speed * Time.deltaTime, 0);
        this.transform.Translate(velocity);
    }

    public void OnBecameInvisible()
    {
        Destroy(this.gameObject);
    }

}
