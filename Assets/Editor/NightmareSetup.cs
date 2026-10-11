using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

public static class NightmareSetup
{
    public const string Output = "Assets/NightmareGenerated";
    public const string ScenePath = Output + "/NightmareToySurvival.unity";
    const int FloorLayer = 8, WorldLayer = 13, PlayerLayer = 14, EnemyLayer = 15;

    [MenuItem("Nightmare/1. Create playable game")]
    public static void CreateGame()
    {
        if (EditorApplication.isPlaying) throw new InvalidOperationException("Stop Play mode first.");
        if (!Application.unityVersion.StartsWith("2018.4."))
            throw new InvalidOperationException("This project targets Unity 2018.4. Open it with 2018.4.17f1; do not upgrade it automatically.");
        if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath))
        {
            EditorSceneManager.OpenScene(ScenePath);
            Debug.Log("Existing game opened. Generated assets are preserved. Edit the scene/prefabs directly.");
            return;
        }
        Directory.CreateDirectory(Output); AssetDatabase.Refresh();
        foreach (string character in new[] { "Player", "Zombunny", "ZomBear", "Hellephant" })
        {
            var importer = (ModelImporter)AssetImporter.GetAtPath("Assets/Models/Characters/" + character + ".fbx");
            if (importer.optimizeGameObjects) { importer.optimizeGameObjects = false; importer.SaveAndReimport(); }
        }
        SetLayer(FloorLayer, "Floor"); SetLayer(WorldLayer, "NightmareWorld");
        SetLayer(PlayerLayer, "NightmarePlayer"); SetLayer(EnemyLayer, "NightmareEnemy");
        Physics.IgnoreLayerCollision(PlayerLayer, EnemyLayer, true);
        Physics.IgnoreLayerCollision(EnemyLayer, EnemyLayer, true); // Agent avoidance handles crowds.
        Physics.IgnoreLayerCollision(PlayerLayer, WorldLayer, false);
        Physics.IgnoreLayerCollision(PlayerLayer, FloorLayer, false);
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        GameObject environment = (GameObject)PrefabUtility.InstantiatePrefab(Load<GameObject>("Assets/Prefabs/Environment.prefab"));
        environment.name = "Toy Room - original Environment";
        SetLayers(environment, WorldLayer);
        foreach (Transform child in environment.GetComponentsInChildren<Transform>(true))
        {
            GameObjectUtility.SetStaticEditorFlags(child.gameObject, 0);
            if (child.name == "Floor")
            {
                SetLayers(child.gameObject, FloorLayer);
                // The room has its original visual floor. Use one level collision plane.
                foreach (Collider collider in child.GetComponentsInChildren<Collider>()) collider.enabled = false;
            }
        }
        GameObject floor = new GameObject("Walkable floor collision"); floor.layer = FloorLayer;
        BoxCollider ground = floor.AddComponent<BoxCollider>(); ground.center = new Vector3(0,-.15f,0); ground.size = new Vector3(48,.3f,46);
        Wall(environment.transform, new Vector3(-24,2,0), new Vector3(1,4,47));
        Wall(environment.transform, new Vector3(24,2,0), new Vector3(1,4,47));
        Wall(environment.transform, new Vector3(0,2,-23), new Vector3(49,4,1));
        Wall(environment.transform, new Vector3(0,2,23), new Vector3(49,4,1));
        Physics.SyncTransforms();
        NavMeshData data = Bake(environment.transform, floor.transform);
        var navInstance = NavMesh.AddNavMeshData(data);
        try
        {
            Vector3 playerPosition = FindStart();
            var playerPrefab = CreatePlayer();
            var bunny = CreateEnemy("Zombunny", 50, 4.5f, 8, 10, 1.5f, false);
            var bear = CreateEnemy("ZomBear", 100, 3.1f, 12, 20, 1.9f, false);
            var elephant = CreateEnemy("Hellephant", 220, 2.0f, 22, 40, 2.5f, false);
            var boss = CreateEnemy("Hellephant", 1200, 2.6f, 30, 500, 3.6f, true);
            PlayerHealth player = ((GameObject)PrefabUtility.InstantiatePrefab(playerPrefab)).GetComponent<PlayerHealth>();
            player.transform.position = playerPosition;
            GameObject systems = new GameObject("Game Systems");
            GameManager game = systems.AddComponent<GameManager>();
            game.player = player; game.score = systems.AddComponent<ScoreManager>(); game.gameOver = systems.AddComponent<GameOverManager>();
            EnemyManager waves = systems.AddComponent<EnemyManager>(); game.waves = waves;
            waves.bunnyPrefab = bunny; waves.bearPrefab = bear; waves.elephantPrefab = elephant; waves.bossPrefab = boss;
            waves.obstructionMask = 1 << WorldLayer; waves.occupiedMask = 1 << EnemyLayer;
            waves.spawnPoints = CreateSpawnPoints(playerPosition);
            ArenaNavigation nav = systems.AddComponent<ArenaNavigation>(); nav.data = data;
            AudioManager audio = systems.AddComponent<AudioManager>();
            audio.music = systems.AddComponent<AudioSource>(); audio.effects = systems.AddComponent<AudioSource>();
            audio.music.playOnAwake = audio.effects.playOnAwake = false;
            audio.music.spatialBlend = audio.effects.spatialBlend = 0;
            audio.backgroundMusic = Load<AudioClip>("Assets/Audio/Music/Background Music.mp3");
            Camera camera = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener)).GetComponent<Camera>();
            camera.tag = "MainCamera"; camera.orthographic = true; camera.orthographicSize = 12;
            camera.nearClipPlane = .1f; camera.farClipPlane = 140; camera.backgroundColor = new Color(.025f,.03f,.075f);
            camera.clearFlags = CameraClearFlags.SolidColor;
            CameraFollow follow = camera.gameObject.AddComponent<CameraFollow>(); follow.target = player.transform;
            camera.transform.position = playerPosition + follow.offset; camera.transform.rotation = Quaternion.LookRotation(-follow.offset);
            player.GetComponent<PlayerMovement>().viewCamera = camera;
            Object.Instantiate(Load<GameObject>("Assets/Prefabs/Lights.prefab"));
            Light moon = new GameObject("Moon fill").AddComponent<Light>(); moon.type = LightType.Directional;
            moon.color = new Color(.45f,.57f,.9f); moon.intensity = .6f; moon.shadows = LightShadows.Soft;
            moon.transform.rotation = Quaternion.Euler(55,-30,0);
            Light warm = new GameObject("Bedside warm light").AddComponent<Light>(); warm.type = LightType.Point;
            warm.transform.position = new Vector3(9,7,12); warm.color = new Color(1,.56f,.3f); warm.intensity = 2; warm.range = 23;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(.25f,.27f,.37f); RenderSettings.fog = false;
            GameUI ui = new GameObject("Game UI", typeof(RectTransform)).AddComponent<GameUI>(); ui.BuildUI();
            PlayerSettings.companyName = "Learning Games"; PlayerSettings.productName = "Nightmare Toy Survival";
            PlayerSettings.defaultScreenWidth = 1280; PlayerSettings.defaultScreenHeight = 720;
            AssetDatabase.SaveAssets();
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            ValidateScene();
            Selection.activeGameObject = systems;
            Debug.Log("Nightmare Toy Survival created. Press Play, then CHƠI. Scene: " + ScenePath);
        }
        finally { if (navInstance.valid) navInstance.Remove(); }
    }

    static void SetLayer(int index, string name)
    {
        SerializedObject tags = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty layer = tags.FindProperty("layers").GetArrayElementAtIndex(index);
        if (!string.IsNullOrEmpty(layer.stringValue) && layer.stringValue != name)
            throw new InvalidOperationException("Layer " + index + " is occupied: " + layer.stringValue);
        layer.stringValue = name; tags.ApplyModifiedProperties();
    }
    static void SetLayers(GameObject root, int layer)
    { foreach (Transform child in root.GetComponentsInChildren<Transform>(true)) child.gameObject.layer = layer; }
    static T Load<T>(string path) where T : Object
    {
        T value = AssetDatabase.LoadAssetAtPath<T>(path);
        if (!value) throw new InvalidOperationException("Missing required asset: " + path);
        return value;
    }
    static void Wall(Transform parent, Vector3 position, Vector3 size)
    {
        GameObject wall = new GameObject("Arena boundary"); wall.transform.SetParent(parent); wall.transform.position = position;
        wall.layer = WorldLayer; wall.AddComponent<BoxCollider>().size = size;
    }
    static NavMeshData Bake(Transform environment, Transform floor)
    {
        var sources = new List<NavMeshBuildSource>(); var obstacles = new List<NavMeshBuildSource>();
        var markups = new List<NavMeshBuildMarkup>();
        UnityEngine.AI.NavMeshBuilder.CollectSources(floor, 1 << FloorLayer, NavMeshCollectGeometry.PhysicsColliders, 0, markups, sources);
        UnityEngine.AI.NavMeshBuilder.CollectSources(environment, 1 << WorldLayer, NavMeshCollectGeometry.PhysicsColliders, 1, markups, obstacles);
        sources.AddRange(obstacles);
        NavMeshBuildSettings settings = NavMesh.GetSettingsByID(0);
        settings.agentRadius = 1; settings.agentHeight = 2; settings.agentClimb = .15f; settings.agentSlope = 35;
        settings.overrideVoxelSize = true; settings.voxelSize = .12f;
        NavMeshData data = UnityEngine.AI.NavMeshBuilder.BuildNavMeshData(settings, sources,
            new Bounds(new Vector3(0,3,0),new Vector3(52,14,50)), Vector3.zero, Quaternion.identity);
        if (!data) throw new InvalidOperationException("NavMesh generation failed.");
        string path = Output + "/ArenaNavMesh.asset";
        if (AssetDatabase.LoadAssetAtPath<NavMeshData>(path)) AssetDatabase.DeleteAsset(path);
        AssetDatabase.CreateAsset(data,path); return data;
    }
    static Vector3 FindStart()
    {
        for (int radius = 0; radius < 10; radius++)
        for (int x = -radius; x <= radius; x++)
        for (int z = -radius; z <= radius; z++)
        {
            NavMeshHit hit;
            if (NavMesh.SamplePosition(new Vector3(x,0,z),out hit,.6f,NavMesh.AllAreas) &&
                !Physics.CheckCapsule(hit.position + Vector3.up*.6f,hit.position + Vector3.up*1.4f,.5f,1<<WorldLayer)) return hit.position;
        }
        throw new InvalidOperationException("No valid player start on the baked NavMesh.");
    }
    static Transform[] CreateSpawnPoints(Vector3 start)
    {
        var points = new List<Transform>(); GameObject parent = new GameObject("Validated Spawn Points");
        NavMeshPath path = new NavMeshPath();
        for (int x = -20; x <= 20; x += 4)
        for (int z = -19; z <= 19; z += 4)
        {
            if (Mathf.Abs(x) < 14 && Mathf.Abs(z) < 13) continue;
            NavMeshHit hit;
            if (!NavMesh.SamplePosition(new Vector3(x,0,z),out hit,1,NavMesh.AllAreas)) continue;
            if (Physics.CheckCapsule(hit.position + Vector3.up*1.05f,hit.position + Vector3.up*1.8f,1,1<<WorldLayer)) continue;
            if (!NavMesh.CalculatePath(hit.position,start,NavMesh.AllAreas,path) || path.status != NavMeshPathStatus.PathComplete) continue;
            GameObject point = new GameObject("Spawn " + points.Count); point.transform.SetParent(parent.transform); point.transform.position = hit.position;
            points.Add(point.transform);
        }
        if (points.Count < 6) throw new InvalidOperationException("Too few reachable spawn points: " + points.Count);
        return points.ToArray();
    }
    static GameObject Model(string name, float desiredHeight)
    {
        GameObject model = Object.Instantiate(Load<GameObject>("Assets/Models/Characters/" + name + ".fbx")); model.name = name;
        var renderers = model.GetComponentsInChildren<Renderer>();
        Bounds bounds = renderers[0].bounds;
        foreach (Renderer r in renderers) bounds.Encapsulate(r.bounds);
        model.transform.localScale *= desiredHeight / Mathf.Max(.1f,bounds.size.y);
        Animator anim = model.GetComponent<Animator>(); if (!anim) anim = model.AddComponent<Animator>();
        anim.applyRootMotion = false; anim.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        anim.runtimeAnimatorController = Controller(name,model.transform);
        return model;
    }
    static void Body(GameObject model, float height, float radius, bool player)
    {
        float scale = model.transform.localScale.x;
        CapsuleCollider capsule = model.AddComponent<CapsuleCollider>(); capsule.height = height / scale;
        capsule.radius = radius / scale; capsule.center = Vector3.up * height / (2*scale);
        Rigidbody rb = model.AddComponent<Rigidbody>(); rb.useGravity = false; rb.isKinematic = !player;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezePositionY;
        if (player) { rb.interpolation = RigidbodyInterpolation.Interpolate; rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic; }
    }
    static GameObject CreatePlayer()
    {
        GameObject player = Model("Player",1.85f); player.tag = "Player"; SetLayers(player,PlayerLayer); Body(player,1.8f,.45f,true);
        PlayerMovement movement = player.AddComponent<PlayerMovement>(); movement.floorMask = 1<<FloorLayer;
        PlayerHealth health = player.AddComponent<PlayerHealth>(); health.hurtClip = Effect("Player Hurt"); health.deathClip = Effect("Player Death");
        GameObject muzzle = Object.Instantiate(Load<GameObject>("Assets/Prefabs/GunParticles.prefab")); muzzle.name = "Gun Muzzle";
        muzzle.transform.SetParent(player.transform,false); float scale = player.transform.localScale.x;
        muzzle.transform.localPosition = new Vector3(.22f,1.02f,.8f) / scale; muzzle.transform.localRotation = Quaternion.identity;
        Transform barrelEnd = player.GetComponentsInChildren<Transform>().FirstOrDefault(t => t.name == "GunBarrelEnd");
        if (barrelEnd) { muzzle.transform.SetParent(barrelEnd, true); muzzle.transform.localPosition = Vector3.zero; }
        Vector3 parentScale = muzzle.transform.parent.lossyScale;
        muzzle.transform.localScale = new Vector3(1/parentScale.x,1/parentScale.y,1/parentScale.z); SetLayers(muzzle,PlayerLayer);
        PlayerShooting gun = muzzle.AddComponent<PlayerShooting>(); gun.playerRoot = player.transform;
        gun.hitMask = (1<<WorldLayer) | (1<<EnemyLayer); gun.shotClip = Effect("Player GunShot");
        gun.gunParticles = muzzle.GetComponent<ParticleSystem>(); var main = gun.gunParticles.main; main.playOnAwake = false;
        gun.hitParticlesPrefab = Load<GameObject>("Assets/Prefabs/HitParticles.prefab");
        gun.gunLine = muzzle.GetComponent<LineRenderer>(); if (!gun.gunLine) gun.gunLine = muzzle.AddComponent<LineRenderer>();
        gun.gunLine.sharedMaterial = Load<Material>("Assets/Materials/LineRenderMaterial.mat");
        gun.gunLine.positionCount = 2; gun.gunLine.useWorldSpace = true; gun.gunLine.startWidth = .04f; gun.gunLine.endWidth = .012f; gun.gunLine.enabled = false;
        gun.gunLight = muzzle.GetComponent<Light>(); if (!gun.gunLight) gun.gunLight = muzzle.AddComponent<Light>();
        gun.gunLight.type = LightType.Point; gun.gunLight.color = new Color(1,.85f,.45f); gun.gunLight.intensity = 1.5f; gun.gunLight.range = 3; gun.gunLight.enabled = false;
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(player,Output+"/Player.prefab"); Object.DestroyImmediate(player); return prefab;
    }
    static EnemyHealth CreateEnemy(string name, int hp, float speed, int damage, int score, float height, bool boss)
    {
        GameObject enemy = Model(name,height); SetLayers(enemy,EnemyLayer); Body(enemy,height,boss?.85f:.5f,false);
        NavMeshAgent agent = enemy.AddComponent<NavMeshAgent>(); agent.agentTypeID = 0; agent.radius = boss?.9f:.55f; agent.height = height;
        agent.speed = speed; agent.angularSpeed = 720; agent.acceleration = 20; agent.stoppingDistance = boss?1.45f:1.05f;
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
        EnemyHealth health = enemy.AddComponent<EnemyHealth>(); health.startingHealth = hp; health.scoreValue = score; health.isBoss = boss;
        string audioName = name == "Zombunny" ? "ZomBunny" : name;
        health.hurtClip = Effect(audioName+" Hurt"); health.deathClip = Effect(audioName+" Death");
        enemy.AddComponent<EnemyMovement>().speed = speed;
        EnemyAttack attack = enemy.AddComponent<EnemyAttack>(); attack.attackDamage = damage; attack.timeBetweenAttacks = boss?1.4f:1;
        attack.attackRange = boss?2:1.55f; attack.obstructionMask = 1<<WorldLayer;
        string assetName = boss?"HellephantBoss":name;
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(enemy,Output+"/"+assetName+".prefab"); Object.DestroyImmediate(enemy);
        return prefab.GetComponent<EnemyHealth>();
    }
    static AudioClip Effect(string name) { return Load<AudioClip>("Assets/Audio/Effects/"+name+".wav"); }
    static RuntimeAnimatorController Controller(string name, Transform rig)
    {
        string path = Output + "/" + name + ".controller";
        RuntimeAnimatorController existing = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(path); if (existing) return existing;
        var controller = AnimatorController.CreateAnimatorControllerAtPath(path);
        controller.AddParameter("Moving",AnimatorControllerParameterType.Bool); controller.AddParameter("Dead",AnimatorControllerParameterType.Bool);
        var machine = controller.layers[0].stateMachine;
        var idle = machine.AddState("Idle"); var move = machine.AddState("Move"); var death = machine.AddState("Death");
        idle.motion = Clip(name,"Idle",rig); move.motion = Clip(name,"Move",rig); death.motion = Clip(name,"Death",rig);
        machine.defaultState = idle;
        AnimatorStateTransition walk = idle.AddTransition(move); walk.hasExitTime = false; walk.duration = .12f;
        walk.AddCondition(AnimatorConditionMode.If,0,"Moving"); walk.AddCondition(AnimatorConditionMode.IfNot,0,"Dead");
        AnimatorStateTransition stop = move.AddTransition(idle); stop.hasExitTime = false; stop.duration = .12f;
        stop.AddCondition(AnimatorConditionMode.IfNot,0,"Moving"); stop.AddCondition(AnimatorConditionMode.IfNot,0,"Dead");
        AnimatorStateTransition die = machine.AddAnyStateTransition(death); die.hasExitTime = false; die.duration = .08f; die.canTransitionToSelf = false;
        die.AddCondition(AnimatorConditionMode.If,0,"Dead"); return controller;
    }
    static AnimationClip Clip(string character, string action, Transform rig)
    {
        AnimationClip source = AssetDatabase.LoadAllAssetsAtPath("Assets/Models/Characters/"+character+".fbx").OfType<AnimationClip>().FirstOrDefault(c=>c.name==action);
        if (!source && character == "ZomBear")
        {
            // The supplied bear importer has no clips. Reuse bunny skeleton clips only
            // when all animated bone paths exist in the bear rig.
            AnimationClip shared = AssetDatabase.LoadAllAssetsAtPath("Assets/Models/Characters/Zombunny.fbx").OfType<AnimationClip>().FirstOrDefault(c=>c.name==action);
            if (shared && AnimationUtility.GetCurveBindings(shared).Where(b=>b.type==typeof(Transform)).All(b=>string.IsNullOrEmpty(b.path)||rig.Find(b.path))) source=shared;
        }
        if (!source) { Debug.LogWarning(character+" has no compatible "+action+" animation. Gameplay still works."); return null; }
        string path = Output+"/"+character+"_"+action+".anim";
        AnimationClip copy = Object.Instantiate(source); copy.name=character+"_"+action;
        AnimationUtility.SetAnimationEvents(copy,new AnimationEvent[0]);
        var settings = AnimationUtility.GetAnimationClipSettings(copy); settings.loopTime = action!="Death"; AnimationUtility.SetAnimationClipSettings(copy,settings);
        AssetDatabase.CreateAsset(copy,path); return copy;
    }

    [MenuItem("Nightmare/2. Validate scene")]
    public static void ValidateScene()
    {
        GameManager game = Object.FindObjectOfType<GameManager>();
        if (!game || !game.player || !game.waves || !game.score || !game.gameOver) throw new InvalidOperationException("Game references incomplete.");
        if (game.waves.waves.Length != 10 || !game.waves.waves[9].boss) throw new InvalidOperationException("Expected 10 waves and final boss.");
        if (game.waves.spawnPoints.Length < 6) throw new InvalidOperationException("Spawn points missing.");
        if (!Object.FindObjectOfType<ArenaNavigation>().data) throw new InvalidOperationException("Baked NavMesh missing.");
        foreach (GameObject root in SceneManager.GetActiveScene().GetRootGameObjects())
        foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
        {
            if (t.GetComponents<Component>().Any(c=>c==null)) throw new InvalidOperationException("Missing script: "+t.name);
        }
        foreach (EnemyHealth prefab in new[] {game.waves.bunnyPrefab,game.waves.bearPrefab,game.waves.elephantPrefab,game.waves.bossPrefab})
            if (!prefab || !prefab.GetComponent<Animator>().runtimeAnimatorController || !prefab.GetComponent<NavMeshAgent>() || !prefab.deathClip)
                throw new InvalidOperationException("Enemy prefab is incomplete.");
        PlayerShooting gun = game.player.GetComponentInChildren<PlayerShooting>();
        if (!gun || !gun.gunParticles || !gun.gunLine || !gun.gunLight || !gun.hitParticlesPrefab || !gun.shotClip)
            throw new InvalidOperationException("Gun references incomplete.");
        if (!Object.FindObjectOfType<GameUI>() || !Object.FindObjectOfType<AudioManager>().backgroundMusic)
            throw new InvalidOperationException("UI/audio missing.");
        GameUI ui = Object.FindObjectOfType<GameUI>();
        foreach (var field in typeof(GameUI).GetFields())
            if (typeof(Object).IsAssignableFrom(field.FieldType) && !(Object)field.GetValue(ui))
                throw new InvalidOperationException("Missing UI reference: " + field.Name);
        Debug.Log("Scene reference validation PASS. This does not replace a gameplay test.");
    }
    [MenuItem("Nightmare/5. Rebake navigation after editing arena")]
    public static void RebakeNavigation()
    {
        if (EditorApplication.isPlaying) throw new InvalidOperationException("Stop Play mode first.");
        GameManager game = Object.FindObjectOfType<GameManager>();
        GameObject environment = GameObject.Find("Toy Room - original Environment");
        GameObject floor = GameObject.Find("Walkable floor collision");
        if (!game || !environment || !floor) throw new InvalidOperationException("Open the generated game scene first.");
        Physics.SyncTransforms(); NavMeshData data = Bake(environment.transform, floor.transform);
        Object.FindObjectOfType<ArenaNavigation>().data = data;
        var instance = NavMesh.AddNavMeshData(data);
        try
        {
            Vector3 start = FindStart(); game.player.transform.position = start;
            GameObject old = GameObject.Find("Validated Spawn Points"); if (old) Object.DestroyImmediate(old);
            game.waves.spawnPoints = CreateSpawnPoints(start);
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene()); AssetDatabase.SaveAssets();
        }
        finally { if (instance.valid) instance.Remove(); }
    }
    [MenuItem("Nightmare/3. Build Windows x64")]
    public static void BuildWindows()
    {
        if (!AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath)) CreateGame();
        EditorSceneManager.OpenScene(ScenePath); ValidateScene();
        string folder = Path.GetFullPath("Builds/Windows"); Directory.CreateDirectory(folder);
        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions { scenes = new[] { ScenePath },
            locationPathName = folder+"/NightmareToySurvival.exe", target = BuildTarget.StandaloneWindows64, options = BuildOptions.None });
        if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
            throw new InvalidOperationException("Windows build failed: "+report.summary.result);
        Debug.Log("Windows build succeeded: "+folder);
    }
}
