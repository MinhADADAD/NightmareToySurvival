using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerShooting : MonoBehaviour
{
    public int damagePerShot = 25;
    public float timeBetweenBullets = .15f;
    public float range = 65f;
    public LayerMask hitMask;
    public Transform playerRoot;
    public ParticleSystem gunParticles;
    public GameObject hitParticlesPrefab;
    public AudioClip shotClip;
    public LineRenderer gunLine;
    public Light gunLight;
    float nextShot, hideAt;
    void Awake() { DisableEffects(); }
    void Update()
    {
        if (!GameManager.IsPlaying) { DisableEffects(); return; }
        if (Time.time >= hideAt) DisableEffects();
        if (Input.GetMouseButton(0) && Time.time >= nextShot &&
            !(EventSystem.current && EventSystem.current.IsPointerOverGameObject())) Shoot();
    }
    public void DisableEffects()
    {
        if (gunLine) gunLine.enabled = false;
        if (gunLight) gunLight.enabled = false;
    }
    void Shoot()
    {
        nextShot = Time.time + Mathf.Max(.04f, timeBetweenBullets); hideAt = Time.time + .045f;
        AudioManager.Instance.Play(shotClip);
        gunParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear); gunParticles.Play();
        gunLight.enabled = true;
        Vector3 origin = transform.position;
        Vector3 shoulder = playerRoot.position; shoulder.y = origin.y;
        Vector3 barrel = origin - shoulder;
        RaycastHit hit;
        // Check body-to-muzzle too, so a barrel through a wall cannot shoot through it.
        bool blocked = Physics.Raycast(shoulder, barrel.normalized, out hit, barrel.magnitude, hitMask, QueryTriggerInteraction.Ignore);
        if (!blocked) blocked = Physics.Raycast(origin, playerRoot.forward, out hit, range, hitMask, QueryTriggerInteraction.Ignore);
        Vector3 end = blocked ? hit.point : origin + playerRoot.forward * range;
        if (blocked)
        {
            EnemyHealth enemy = hit.collider.GetComponentInParent<EnemyHealth>();
            if (enemy) enemy.TakeDamage(damagePerShot, hit.point);
            GameObject effect = Instantiate(hitParticlesPrefab, hit.point, Quaternion.LookRotation(hit.normal));
            effect.GetComponent<ParticleSystem>().Play(); Destroy(effect, 2);
        }
        gunLine.enabled = true; gunLine.SetPosition(0, origin); gunLine.SetPosition(1, end);
    }
}
