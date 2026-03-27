using UnityEngine;

public class BobAnimation : MonoBehaviour
{
    public float bobSpeed = 2f;      // How fast it bobs
    public float bobHeight = 10f;    // How far it moves up/down
    
    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.localPosition;
    }

    void Update()
    {
        float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.localPosition = new Vector3(startPosition.x, newY, startPosition.z);
    }
}
