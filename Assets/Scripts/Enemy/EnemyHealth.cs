using UnityEngine;
using UnityEngine.AI;

public class EnemyHealth : MonoBehaviour
{
    public int startingHealth = 100;
    public int currentHealth;
    public int scoreValue = 20;
    public bool isBoss;
    public AudioClip hurtClip, deathClip;
    public float corpseSeconds = 2;
    public bool IsDead { get { return dead; } }
    public EnemyManager Owner { get; set; }
    bool dead;
    float deathTime, nextHurtSound;
    Animator anim;
    void Awake() { currentHealth = startingHealth; anim = GetComponent<Animator>(); }
    public void TakeDamage(int amount, Vector3 hitPoint)
    {
        if (dead || !GameManager.IsPlaying || amount <= 0) return;
        currentHealth = Mathf.Max(0, currentHealth - amount);
        if (currentHealth == 0)
        {
            dead = true; // Guard BEFORE score and wave callbacks: award exactly once.
            deathTime = Time.time;
            GetComponent<NavMeshAgent>().enabled = false;
            foreach (Collider c in GetComponentsInChildren<Collider>()) c.enabled = false;
            anim.SetBool("Dead", true); AudioManager.Instance.Play(deathClip);
            if (Owner) Owner.EnemyDied(this);
            Destroy(gameObject, corpseSeconds + 1.2f);
        }
        else if (Time.time >= nextHurtSound)
        {
            nextHurtSound = Time.time + .2f; AudioManager.Instance.Play(hurtClip);
        }
    }
    void Update()
    {
        if (dead && Time.time > deathTime + corpseSeconds) transform.position += Vector3.down * Time.deltaTime;
    }
    // Legacy animation event; deterministic cleanup above works without an event.
    public void StartSinking() { }
}
