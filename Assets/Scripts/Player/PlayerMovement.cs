using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    public float speed = 6f;
    public LayerMask floorMask;
    public Camera viewCamera;
    Rigidbody body;
    Animator anim;
    Vector3 input;
    Quaternion aim;
    void Awake() { body = GetComponent<Rigidbody>(); anim = GetComponent<Animator>(); aim = transform.rotation; }
    void Update()
    {
        input = Vector3.zero;
        if (!GameManager.IsPlaying) { anim.SetBool("Moving", false); return; }
        input = Vector3.ClampMagnitude(new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")), 1);
        if (viewCamera)
        {
            Ray ray = viewCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            Vector3 aimPoint;
            if (Physics.Raycast(ray, out hit, 300, floorMask, QueryTriggerInteraction.Ignore)) aimPoint = hit.point;
            else
            {
                // Keep aiming responsive when the pointer is beyond the floor collider.
                float distance;
                if (!new Plane(Vector3.up, transform.position).Raycast(ray, out distance)) return;
                aimPoint = ray.GetPoint(distance);
            }
            Vector3 direction = aimPoint - transform.position; direction.y = 0;
            if (direction.sqrMagnitude > .01f) aim = Quaternion.LookRotation(direction);
        }
        anim.SetBool("Moving", input.sqrMagnitude > .01f);
    }
    void FixedUpdate()
    {
        // PhysX resolves walls; normalized input prevents diagonal speed bonus.
        body.velocity = GameManager.IsPlaying ? input * speed : Vector3.zero;
        if (GameManager.IsPlaying) body.MoveRotation(aim);
    }
}
