using System;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool {
    private GameObject parent;
    private PoolableObject prefab;
    private int size;
    private List<PoolableObject> availableObjectsPool;
    private List<PoolableObject> activeObjects;
    private static Dictionary<PoolableObject, ObjectPool> objectPools = new Dictionary<PoolableObject, ObjectPool>();

    private ObjectPool(PoolableObject prefab, int size) {
        this.prefab = prefab;
        this.size = size;
        availableObjectsPool = new List<PoolableObject>(size);
        activeObjects = new List<PoolableObject>(size);
    }

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

    private void CreateObjects() {
        for (int i = 0; i < size; i++) {
            CreateObject();
        }
    }

    private void CreateObject() {
        PoolableObject poolableObject = GameObject.Instantiate(prefab, Vector3.zero, Quaternion.identity, parent.transform);
        poolableObject.parent = this;
        poolableObject.gameObject.SetActive(false);
        availableObjectsPool.Add(poolableObject);
    }

    private void CleanupLists() {
        availableObjectsPool.RemoveAll(item => item == null || item.gameObject == null);
        activeObjects.RemoveAll(item => item == null || item.gameObject == null);
    }

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

    public PoolableObject GetObject() {
        return GetObject(Vector3.zero, Quaternion.identity);
    }

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
