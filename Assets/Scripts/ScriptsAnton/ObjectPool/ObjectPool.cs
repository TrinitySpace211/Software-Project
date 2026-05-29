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

    public PoolableObject GetObject(Vector3 position, Quaternion rotation) {
        if (availableObjectsPool.Count == 0) { //Wenn es keine Objekte mehr im Pool hat, dann soll automatisch mehr hinzugefügt werden
            CreateObject();
        }

        // Wenn die maximale Anzahl aktiver Objekte erreicht ist, deaktivieren wir das älteste aktive Objekt (FIFO)
        if (activeObjects.Count >= size) {
            PoolableObject oldest = activeObjects[0];
            // Entferne aus aktiven Liste bevor deaktivieren, OnDisable() ruft ReturnObjectToPool
            activeObjects.RemoveAt(0);
            if (oldest != null && oldest.gameObject.activeSelf) {
                oldest.gameObject.SetActive(false);
            }
        }

        PoolableObject instance = availableObjectsPool[0];
        availableObjectsPool.RemoveAt(0);

        instance.transform.position = position;
        instance.transform.rotation = rotation;

        activeObjects.Add(instance);

        instance.gameObject.SetActive(true);

        return instance;
    }

    public PoolableObject GetObject() {
        return GetObject(Vector3.zero, Quaternion.identity);
    }

    public void ReturnObjectToPool(PoolableObject Object) {
        // Remove from active list if present
        if (activeObjects.Contains(Object)) {
            activeObjects.Remove(Object);
        }

        // Avoid duplicates in available pool
        if (!availableObjectsPool.Contains(Object)) {
            availableObjectsPool.Add(Object);
        }
    }
}
