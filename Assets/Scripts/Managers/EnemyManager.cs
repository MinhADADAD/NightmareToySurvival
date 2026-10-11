using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[Serializable]
public class NightmareWave
{
    public int bunnies, bears, elephants;
    public bool boss;
    public float spawnInterval = .55f;
    public NightmareWave(int a, int b, int c, bool d = false) { bunnies = a; bears = b; elephants = c; boss = d; }
}

public class EnemyManager : MonoBehaviour
{
    public EnemyHealth bunnyPrefab, bearPrefab, elephantPrefab, bossPrefab;
    public Transform[] spawnPoints;
    public LayerMask obstructionMask, occupiedMask;
    public float breakSeconds = 5;
    public float minPlayerDistance = 10;
    public int healBetweenWaves = 15;
    public float healthGrowthPerWave = .07f;
    public NightmareWave[] waves = {
        new NightmareWave(5,0,0), new NightmareWave(6,2,0), new NightmareWave(7,3,0),
        new NightmareWave(6,4,1), new NightmareWave(8,4,2), new NightmareWave(8,5,3),
        new NightmareWave(10,6,3), new NightmareWave(10,7,4), new NightmareWave(12,8,4),
        new NightmareWave(10,8,5,true) };
    public int WaveNumber { get; private set; }
    public int Remaining { get { return living.Count + pending; } }
    public float Countdown { get; private set; }
    public EnemyHealth Boss { get; private set; }
    public string SpawnWarning { get; private set; }
    readonly HashSet<EnemyHealth> living = new HashSet<EnemyHealth>();
    NavMeshPath path;
    int pending;
    bool begun;

    // NavMeshPath allocates a native Unity object: never construct it in a field
    // initializer, which Unity can run on its serialization/loading thread.
    void Awake() { path = new NavMeshPath(); }

    public void Begin() { if (begun) return; begun = true; StartCoroutine(RunWaves()); }
    IEnumerator RunWaves()
    {
        for (int i = 0; i < waves.Length; i++)
        {
            WaveNumber = i + 1;
            Countdown = breakSeconds;
            while (Countdown > 0) { Countdown = Mathf.Max(0, Countdown - Time.deltaTime); yield return null; }
            NightmareWave wave = waves[i];
            var queue = new List<EnemyHealth>();
            for (int j = 0; j < wave.bunnies; j++) queue.Add(bunnyPrefab);
            for (int j = 0; j < wave.bears; j++) queue.Add(bearPrefab);
            for (int j = 0; j < wave.elephants; j++) queue.Add(elephantPrefab);
            // Shuffle the ordinary enemies, spawn the boss first in wave 10.
            for (int j = queue.Count - 1; j > 0; j--)
            { int k = UnityEngine.Random.Range(0, j + 1); EnemyHealth temp = queue[j]; queue[j] = queue[k]; queue[k] = temp; }
            if (wave.boss) queue.Insert(0, bossPrefab);
            pending = queue.Count;
            foreach (EnemyHealth prefab in queue)
            {
                Vector3 position;
                float blockedFor = 0;
                while (!TrySpawnPosition(prefab, out position))
                {
                    blockedFor += .3f;
                    if (blockedFor >= 3) SpawnWarning = "Đang tìm điểm sinh an toàn — hãy di chuyển ra giữa phòng.";
                    yield return new WaitForSeconds(.3f);
                }
                SpawnWarning = "";
                EnemyHealth enemy = Instantiate(prefab, position, Quaternion.identity);
                enemy.startingHealth = Mathf.RoundToInt(prefab.startingHealth * (1 + i * healthGrowthPerWave));
                enemy.currentHealth = enemy.startingHealth; enemy.Owner = this;
                living.Add(enemy); pending--;
                if (enemy.isBoss) Boss = enemy;
                yield return new WaitForSeconds(Mathf.Max(.1f, wave.spawnInterval));
            }
            while (living.Count > 0) yield return null;
            if (i < waves.Length - 1)
            {
                PlayerHealth player = GameManager.Instance.player;
                player.currentHealth = Mathf.Min(player.startingHealth, player.currentHealth + healBetweenWaves);
            }
        }
        GameManager.Instance.Finish(true);
    }

    public bool TrySpawnPosition(EnemyHealth prefab, out Vector3 position)
    {
        position = Vector3.zero;
        if (spawnPoints == null || spawnPoints.Length == 0) return false;
        Vector3 target = GameManager.Instance.player.transform.position;
        NavMeshHit targetHit;
        if (!NavMesh.SamplePosition(target, out targetHit, 2, NavMesh.AllAreas)) return false;
        int first = UnityEngine.Random.Range(0, spawnPoints.Length);
        float radius = prefab.GetComponent<CapsuleCollider>().radius * prefab.transform.localScale.x + .15f;
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            NavMeshHit hit;
            if (!NavMesh.SamplePosition(spawnPoints[(first + i) % spawnPoints.Length].position, out hit, 1, NavMesh.AllAreas)) continue;
            if ((hit.position - target).sqrMagnitude < minPlayerDistance * minPlayerDistance) continue;
            if (Physics.CheckCapsule(hit.position + Vector3.up * (radius + .1f), hit.position + Vector3.up * 1.8f,
                radius, obstructionMask | occupiedMask, QueryTriggerInteraction.Ignore)) continue;
            if (!NavMesh.CalculatePath(hit.position, targetHit.position, NavMesh.AllAreas, path) || path.status != NavMeshPathStatus.PathComplete) continue;
            position = hit.position; return true;
        }
        return false;
    }
    public void EnemyDied(EnemyHealth enemy)
    {
        if (!living.Remove(enemy)) return;
        GameManager.Instance.score.Add(enemy.scoreValue);
    }
    void OnDrawGizmosSelected()
    {
        if (spawnPoints == null) return;
        Gizmos.color = Color.cyan;
        foreach (Transform point in spawnPoints) if (point) Gizmos.DrawWireSphere(point.position + Vector3.up, .9f);
    }
}
