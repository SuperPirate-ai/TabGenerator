using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

public class Note : MonoBehaviour
{
    private NoteManager manager;
    private int bpm;
    private int speed;
    public delegate void MouseClickNote([Optional] GameObject _note);
    public static event MouseClickNote clickedOnMouse;
    private void OnEnable()
    {
        manager = NoteManager.Instance;
        bpm = manager.BPM;
        speed = manager.NoteSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        if (!manager.playedNotes.Contains(this.gameObject)) Destroy(this.gameObject);

        if (this.gameObject == NoteManager.Instance.playedNotes.Last() && this.transform.position.x < 0 && !NoteManager.Instance.IsRecording && !NoteManager.Instance.PlayPaused)
        {
            Move();
            NoteManager.Instance.Play();
        }
        Move();
    }
    void Move()
    {
        if (NoteManager.Instance.PlayPaused) return;

        Vector3 velocity = new Vector3(-speed * Time.deltaTime, 0);
        this.transform.Translate(velocity);
    }

    private void OnMouseDown()
    {
        //clickedOnMouse.Invoke(this.gameObject);
    }

}
