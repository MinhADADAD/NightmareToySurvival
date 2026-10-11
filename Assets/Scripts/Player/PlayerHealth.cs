using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int startingHealth = 100;
    public int currentHealth;
    public AudioClip hurtClip, deathClip;
    public float invulnerabilitySeconds = .35f;
    public float LastHitTime { get; private set; }
    public bool IsDead { get { return currentHealth <= 0; } }
    float nextDamageTime;
    Animator anim;
    void Awake() { currentHealth = startingHealth; LastHitTime = -100; anim = GetComponent<Animator>(); }
    public void TakeDamage(int amount)
    {
        if (!GameManager.IsPlaying || IsDead || amount <= 0 || Time.time < nextDamageTime) return;
        nextDamageTime = Time.time + invulnerabilitySeconds; LastHitTime = Time.time;
        currentHealth = Mathf.Max(0, currentHealth - amount);
        AudioManager.Instance.Play(IsDead ? deathClip : hurtClip);
        if (IsDead)
        {
            anim.SetBool("Dead", true);
            GetComponent<Rigidbody>().velocity = Vector3.zero;
            GetComponentInChildren<PlayerShooting>().DisableEffects();
            GameManager.Instance.Finish(false);
        }
    }
    // Original FBX death event must NOT restart automatically. UI owns restart.
    public void RestartLevel() { }
}
