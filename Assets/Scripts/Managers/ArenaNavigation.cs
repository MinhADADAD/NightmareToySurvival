using UnityEngine;
using UnityEngine.AI;

// Baked data is included in the build; no external NavMesh package required.
[DefaultExecutionOrder(-90)]
public class ArenaNavigation : MonoBehaviour
{
    public NavMeshData data;
    NavMeshDataInstance instance;
    void OnEnable() { if (data) instance = NavMesh.AddNavMeshData(data); }
    void OnDisable() { if (instance.valid) instance.Remove(); }
}
