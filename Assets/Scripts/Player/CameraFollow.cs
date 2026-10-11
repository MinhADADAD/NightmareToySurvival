using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0, 19, -15);
    public float smoothTime = .18f;
    Vector3 velocity;
    void Start() { transform.position = target.position + offset; transform.rotation = Quaternion.LookRotation(-offset); }
    void LateUpdate()
    {
        if (target) transform.position = Vector3.SmoothDamp(transform.position, target.position + offset, ref velocity, smoothTime);
    }
}
