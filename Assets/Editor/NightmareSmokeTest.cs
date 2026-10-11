using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using Object = UnityEngine.Object;

// Optional integration test driven by real Unity Play mode, without a test package.
// It does not simulate keyboard/mouse or replace visual gameplay testing.
[InitializeOnLoad]
public static class NightmareSmokeTest
{
    const string Key = "NTS.Smoke.";
    static NightmareSmokeTest()
    {
        EditorApplication.update += Tick;
        EditorApplication.playModeStateChanged += ModeChanged;
    }
    static void ModeChanged(PlayModeStateChange state)
    {
        if (state != PlayModeStateChange.EnteredEditMode || !SessionState.GetBool(Key+"Restore",false)) return;
        // Restore AFTER OnApplicationQuit/OnDestroy, including an interrupted test.
        if (SessionState.GetBool(Key+"HadHighScore",false))
            PlayerPrefs.SetInt("NightmareToySurvival.HighScore",SessionState.GetInt(Key+"HighScore",0));
        else PlayerPrefs.DeleteKey("NightmareToySurvival.HighScore");
        PlayerPrefs.Save(); SessionState.SetBool(Key+"Restore",false);
        if (SessionState.GetBool(Key+"Active",false))
        {
            SessionState.SetBool(Key+"Active",false);
            Directory.CreateDirectory("Verification");
            File.WriteAllText("Verification/playmode-smoke.txt","ABORTED: Play mode stopped before test completion.");
        }
    }
    [MenuItem("Nightmare/4. Run Play Mode smoke test")]
    public static void Run()
    {
        if (EditorApplication.isPlaying) throw new InvalidOperationException("Stop Play mode first.");
        if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        EditorSceneManager.OpenScene(NightmareSetup.ScenePath); NightmareSetup.ValidateScene();
        SessionState.SetBool(Key+"Active",true); SessionState.SetInt(Key+"Step",0);
        SessionState.SetBool(Key+"BossSeen",false);
        SessionState.SetBool(Key+"Restore",true);
        SessionState.SetBool(Key+"HadHighScore",PlayerPrefs.HasKey("NightmareToySurvival.HighScore"));
        SessionState.SetInt(Key+"HighScore",PlayerPrefs.GetInt("NightmareToySurvival.HighScore",0));
        SessionState.SetString(Key+"Start",DateTime.UtcNow.ToString("O"));
        EditorApplication.isPlaying = true;
    }
    static void Tick()
    {
        if (!SessionState.GetBool(Key+"Active",false)) return;
        if (!EditorApplication.isPlaying) return;
        try
        {
            if ((DateTime.UtcNow-DateTime.Parse(SessionState.GetString(Key+"Start",""))).TotalSeconds>120)
                throw new Exception("Timed out: inspect spawn/NavMesh warnings.");
            GameManager game = GameManager.Instance; if (!game) return;
            int step = SessionState.GetInt(Key+"Step",0);
            if (step==0)
            {
                Check(game.State==GameState.Menu && game.player.currentHealth==100 && game.score.Score==0,"Initial state");
                Vector3 spawn;
                foreach (EnemyHealth prefab in new[] { game.waves.bunnyPrefab, game.waves.bearPrefab, game.waves.elephantPrefab, game.waves.bossPrefab })
                    Check(game.waves.TrySpawnPosition(prefab, out spawn), "Spawn query after Awake: " + prefab.name);
                Check(Cursor.visible && Cursor.lockState == CursorLockMode.None, "Visible unlocked aiming cursor");
                RaycastHit wall;
                Check(Physics.Raycast(new Vector3(0,1,0),Vector3.right,out wall,60,LayerMask.GetMask("NightmareWorld")),"Wall raycast");
                NavMeshHit target; Check(NavMesh.SamplePosition(game.player.transform.position,out target,2,NavMesh.AllAreas),"Player start on NavMesh");
                foreach (Transform point in game.waves.spawnPoints)
                {
                    NavMeshPath path = new NavMeshPath();
                    Check(NavMesh.CalculatePath(point.position,target.position,NavMesh.AllAreas,path) && path.status==NavMeshPathStatus.PathComplete,"Reachable spawn");
                }
                FastWaves(game); game.Play(); Next(1);
            }
            else if (step==1)
            {
                EnemyHealth[] enemies = Object.FindObjectsOfType<EnemyHealth>(); if (enemies.Length==0) return;
                EnemyHealth enemy = enemies[0];
                Check(enemy.GetComponent<NavMeshAgent>().isOnNavMesh,"Spawned agent on NavMesh");
                int score=game.score.Score, remaining=game.waves.Remaining;
                enemy.TakeDamage(enemy.currentHealth,enemy.transform.position); enemy.TakeDamage(99999,enemy.transform.position);
                Check(game.score.Score==score+enemy.scoreValue && game.waves.Remaining==remaining-1,"Exactly-once kill");
                Check(!enemy.GetComponent<NavMeshAgent>().enabled && !enemy.GetComponent<Collider>().enabled,"Dead enemy stopped");
                game.player.TakeDamage(25); Check(game.player.currentHealth==75,"Player damage");
                game.Pause(); Check(Time.timeScale==0 && game.State==GameState.Paused,"Pause state");
                game.player.TakeDamage(999); Check(game.player.currentHealth==75,"Pause blocks damage");
                game.Resume(); Check(Time.timeScale==1 && GameManager.IsPlaying,"Resume state");
                SessionState.SetFloat(Key+"DamageAt",Time.time); Next(2);
            }
            else if (step==2)
            {
                if (Time.time-SessionState.GetFloat(Key+"DamageAt",0)<.5f) return;
                game.player.TakeDamage(99999); Check(game.State==GameState.Lost && game.player.currentHealth==0,"Loss");
                SessionState.SetInt(Key+"OldInstance",game.GetInstanceID()); Next(3); game.Restart();
            }
            else if (step==3)
            {
                if (game.GetInstanceID()==SessionState.GetInt(Key+"OldInstance",0) || !GameManager.IsPlaying) return;
                Check(game.score.Score==0 && game.player.currentHealth==100 && Time.timeScale==1,"Restart resets score/HP/time");
                FastWaves(game); Next(4);
            }
            else if (step==4)
            {
                // Remove combat pressure, then exercise all real wave transitions.
                game.player.invulnerabilitySeconds=999;
                foreach (EnemyHealth enemy in Object.FindObjectsOfType<EnemyHealth>())
                {
                    if (enemy.isBoss) SessionState.SetBool(Key+"BossSeen",true);
                    enemy.TakeDamage(999999,enemy.transform.position);
                }
                if (game.State!=GameState.Won) return;
                Check(game.waves.WaveNumber==10 && game.waves.Remaining==0 && SessionState.GetBool(Key+"BossSeen",false),"Ten waves and boss victory");
                SessionState.SetInt(Key+"OldInstance",game.GetInstanceID()); Next(5); game.Restart();
            }
            else if (step==5)
            {
                if (game.GetInstanceID()==SessionState.GetInt(Key+"OldInstance",0) || !GameManager.IsPlaying) return;
                game.Pause(); SessionState.SetInt(Key+"OldInstance",game.GetInstanceID()); Next(6); game.Restart();
            }
            else if (step==6)
            {
                if (game.GetInstanceID()==SessionState.GetInt(Key+"OldInstance",0) || !GameManager.IsPlaying) return;
                Check(Time.timeScale==1 && game.score.Score==0 && game.player.currentHealth==100,"Restart from pause");
                SessionState.SetInt(Key+"OldInstance",game.GetInstanceID()); Next(7); game.Menu();
            }
            else if (step==7)
            {
                if (game.GetInstanceID()==SessionState.GetInt(Key+"OldInstance",0)) return;
                Check(game.State==GameState.Menu && Time.timeScale==1,"Return to menu");
                Finish("PASS: initial state, wall raycast, spawn paths, agent placement, one-time score, dead enemy, damage, pause/resume, defeat, restarts, 10 waves, boss, victory, menu.\nKeyboard/mouse, visuals, animation, audio and Windows build still require manual verification.");
            }
        }
        catch (Exception error) { Finish("FAIL: "+error); }
    }
    static void Next(int step) { SessionState.SetInt(Key+"Step",step); }
    static void FastWaves(GameManager game)
    {
        game.waves.breakSeconds=.02f; foreach (NightmareWave wave in game.waves.waves) wave.spawnInterval=.1f;
    }
    static void Check(bool condition,string label) { if (!condition) throw new Exception(label); }
    static void Finish(string result)
    {
        SessionState.SetBool(Key+"Active",false);
        Directory.CreateDirectory("Verification"); File.WriteAllText("Verification/playmode-smoke.txt",DateTime.Now+"\n"+result);
        Debug.Log(result); EditorApplication.isPlaying=false;
        if (Application.isBatchMode) EditorApplication.delayCall += delegate { EditorApplication.Exit(result.StartsWith("PASS") ? 0 : 1); };
    }
}
