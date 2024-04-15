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
    }
    public void Move()
    {
        if (manager.PlayPaused) return;
        speed = (int)(manager.NoteSpeed * (manager.BPM / 100f));

        Vector3 velocity = new Vector3(-speed * Time.deltaTime, 0);
        this.transform.Translate(velocity);
    }
   
}
