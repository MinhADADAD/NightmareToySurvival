using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyMovement : MonoBehaviour
{
    public float speed = 3f;
    public float repathSeconds = .2f;
    NavMeshAgent agent;
    EnemyHealth health;
    Animator anim;
    float nextPath;
    void Awake()
    {
        agent = GetComponent<NavMeshAgent>(); health = GetComponent<EnemyHealth>();
        anim = GetComponent<Animator>(); agent.speed = speed;
    }
    void Update()
    {
        if (!agent.enabled || !agent.isOnNavMesh) return;
        bool active = GameManager.IsPlaying && !health.IsDead;
        agent.isStopped = !active;
        anim.SetBool("Moving", active && agent.velocity.sqrMagnitude > .04f);
        if (!active || Time.time < nextPath) return;
        nextPath = Time.time + repathSeconds;
        NavMeshHit hit;
        // Project a physics-controlled player back onto the agent's walkable surface.
        if (NavMesh.SamplePosition(GameManager.Instance.player.transform.position, out hit, 2, NavMesh.AllAreas))
            agent.SetDestination(hit.position);
    }
}
