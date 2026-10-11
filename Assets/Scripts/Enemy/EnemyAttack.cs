using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    public float timeBetweenAttacks = 1f;
    public int attackDamage = 10;
    public float attackRange = 1.5f;
    public LayerMask obstructionMask;
    EnemyHealth health;
    float nextAttack;
    void Awake() { health = GetComponent<EnemyHealth>(); nextAttack = Time.time + .7f; }
    void Update()
    {
        if (!GameManager.IsPlaying || health.IsDead || Time.time < nextAttack) return;
        PlayerHealth player = GameManager.Instance.player;
        Vector3 delta = player.transform.position - transform.position; delta.y = 0;
        if (delta.sqrMagnitude > attackRange * attackRange) return;
        if (Physics.Linecast(transform.position + Vector3.up, player.transform.position + Vector3.up,
            obstructionMask, QueryTriggerInteraction.Ignore)) return;
        nextAttack = Time.time + Mathf.Max(.1f, timeBetweenAttacks);
        if (delta.sqrMagnitude > .01f) transform.rotation = Quaternion.LookRotation(delta);
        player.TakeDamage(attackDamage);
    }
}
