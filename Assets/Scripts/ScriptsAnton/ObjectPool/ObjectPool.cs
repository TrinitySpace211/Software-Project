using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Object Pool class to prevent excessive use of the Instantiate and Destroy Functions
/// </summary>
public class ObjectPool {
    private GameObject parent;
    private PoolableObject prefab;
    private int size;
    private List<PoolableObject> availableObjectsPool;
    private List<PoolableObject> activeObjects;
    private static Dictionary<PoolableObject, ObjectPool> objectPools = new Dictionary<PoolableObject, ObjectPool>();

    /// <summary>
    /// Constructor for the Object Pool Class
    /// </summary>
    /// <param name="prefab">The Prefab of the Object that can be put in a pool</param>
    /// <param name="size">the amount of objects to generate</param>
    private ObjectPool(PoolableObject prefab, int size) {
        this.prefab = prefab;
        this.size = size;
        availableObjectsPool = new List<PoolableObject>(size);
        activeObjects = new List<PoolableObject>(size);
    }

    /// <summary>
    /// Creates an Object Pool instance
    /// </summary>
    /// <param name="prefab">The Prefab of the Object that can be put in a pool</param>
    /// <param name="size">the amount of objects to generate</param>
    /// <returns>the Object Pool instance with all objects</returns>
    /// <exception cref="ArgumentNullException">Throws an exception when the prefab is null</exception>
    public static ObjectPool CreateInstance(PoolableObject prefab, int size) {
        ObjectPool pool = null;

        if (prefab == null) {
            throw new ArgumentNullException(nameof(prefab), "Prefab cannot be null when creating an ObjectPool.");
        }

        if (objectPools.ContainsKey(prefab)) {
            pool = objectPools[prefab];
        } else {
            pool = new ObjectPool(prefab, size);

            pool.parent = new GameObject(prefab.name + " Pool");
            GameObject.DontDestroyOnLoad(pool.parent);
            pool.CreateObjects();

            objectPools.Add(prefab, pool);
        }

        return pool;
    }

    /// <summary>
    /// Goes through a loop to generate an object depending on the set size
    /// </summary>
    private void CreateObjects() {
        for (int i = 0; i < size; i++) {
            CreateObject();
        }
    }

    /// <summary>
    /// Creates an Poolable object instance and puts it into the List of available Objects
    /// </summary>
    private void CreateObject() {
        PoolableObject poolableObject = GameObject.Instantiate(prefab, Vector3.zero, Quaternion.identity, parent.transform);
        poolableObject.parent = this;
        poolableObject.gameObject.SetActive(false);
        availableObjectsPool.Add(poolableObject);
    }

    /// <summary>
    /// Cleans the Object Pool Lists
    /// </summary>
    private void CleanupLists() {
        availableObjectsPool.RemoveAll(item => item == null || item.gameObject == null);
        activeObjects.RemoveAll(item => item == null || item.gameObject == null);
    }

    /// <summary>
    /// Selects the oldest Object and returns it
    /// </summary>
    /// <param name="position">Where the Object should spawn</param>
    /// <param name="rotation">Which rotation should the object have</param>
    /// <returns></returns>
    public PoolableObject GetObject(Vector3 position, Quaternion rotation) {
        CleanupLists();

        if (availableObjectsPool.Count == 0) {
            CreateObject();
            CleanupLists();
        }

        while (availableObjectsPool.Count > 0) {
            PoolableObject instance = availableObjectsPool[0];
            availableObjectsPool.RemoveAt(0);

            if (instance == null || instance.gameObject == null) {
                CleanupLists();
                continue;
            }

            if (activeObjects.Count >= size) {
                PoolableObject oldest = activeObjects[0];
                activeObjects.RemoveAt(0);
                if (oldest != null && oldest.gameObject != null && oldest.gameObject.activeSelf) {
                    oldest.gameObject.SetActive(false);
                }
            }

            instance.transform.position = position;
            instance.transform.rotation = rotation;

            activeObjects.Add(instance);
            instance.gameObject.SetActive(true);

            return instance;
        }

        CreateObject();
        return GetObject(position, rotation);
    }

    /// <summary>
    /// Picks the oldest Object in the Pool and returns it
    /// The Object spawns at Vector3.zero with a rotation of Quaternion.identity
    /// </summary>
    /// <returns>The oldest Object from the pool</returns>
    public PoolableObject GetObject() {
        return GetObject(Vector3.zero, Quaternion.identity);
    }

    /// <summary>
    /// Puts the Object back to the pool
    /// </summary>
    /// <param name="Object">the Object that should be returned</param>
    public void ReturnObjectToPool(PoolableObject Object) {
        if (Object == null || Object.gameObject == null) {
            CleanupLists();
            return;
        }

        if (activeObjects.Contains(Object)) {
            activeObjects.Remove(Object);
        }

        if (!availableObjectsPool.Contains(Object)) {
            availableObjectsPool.Add(Object);
        }
    }
}
