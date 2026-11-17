using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Implemented by spawned objects that must restore their state when reused.</summary>
public interface ISpawnable
{
    /// <summary>Called after the object is positioned and activated, for new and reused instances.</summary>
    void OnSpawned();
}

/// <summary>
/// Single creation point for enemies, hazards, projectiles and effects.
/// Callers keep their serialized prefab and spawn-point references; the factory
/// only decides whether a released instance can be reused instead of allocated.
/// </summary>
public static class SpawnFactory
{
    private static readonly Dictionary<GameObject, Stack<GameObject>> available =
        new Dictionary<GameObject, Stack<GameObject>>();
    private static readonly Dictionary<GameObject, GameObject> prefabByInstance =
        new Dictionary<GameObject, GameObject>();
    private static readonly HashSet<GameObject> released = new HashSet<GameObject>();
    private static readonly List<ISpawnable> spawnableBuffer = new List<ISpawnable>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Initialize()
    {
        Clear();
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    // Instances belong to their scene, so every reference is stale after an unload.
    private static void OnSceneUnloaded(Scene scene) => Clear();

    private static void Clear()
    {
        available.Clear();
        prefabByInstance.Clear();
        released.Clear();
    }

    /// <summary>Creates or reuses an instance of <paramref name="prefab"/> at the given pose.</summary>
    public static GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (prefab == null)
            return null;

        GameObject instance = null;
        if (available.TryGetValue(prefab, out Stack<GameObject> stack))
        {
            while (stack.Count > 0 && instance == null)
                instance = stack.Pop();
        }

        if (instance == null)
        {
            instance = Object.Instantiate(prefab, position, rotation);
            prefabByInstance[instance] = prefab;
        }
        else
        {
            released.Remove(instance);
            instance.transform.SetPositionAndRotation(position, rotation);
            instance.SetActive(true);
        }

        instance.GetComponentsInChildren(true, spawnableBuffer);
        for (int i = 0; i < spawnableBuffer.Count; i++)
            spawnableBuffer[i].OnSpawned();
        spawnableBuffer.Clear();
        return instance;
    }

    /// <summary>True when the instance was created by the factory and can be reused.</summary>
    public static bool IsManaged(GameObject instance)
    {
        return instance != null && prefabByInstance.ContainsKey(instance);
    }

    /// <summary>
    /// Returns a factory instance for reuse. Objects the factory did not create
    /// are destroyed, so callers can use this in place of Destroy.
    /// </summary>
    public static void Release(GameObject instance)
    {
        if (instance == null)
            return;

        if (!prefabByInstance.TryGetValue(instance, out GameObject prefab))
        {
            Object.Destroy(instance);
            return;
        }

        if (!released.Add(instance))
            return;

        instance.SetActive(false);
        if (!available.TryGetValue(prefab, out Stack<GameObject> stack))
        {
            stack = new Stack<GameObject>();
            available[prefab] = stack;
        }
        stack.Push(instance);
    }
}
