using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Handles the Saveing and Loading of the Game
/// </summary>
public class SaveManager : MonoBehaviour {
    public static SaveManager Instance { get; private set; }

    private readonly List<ISaveable> saveables = new();
    private string savePath;

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        savePath = Path.Combine(Application.persistentDataPath, "save.json");
    }

    private void Start() {
        LoadGame();
    }

    /// <summary>
    /// Registers the classes that implement the ISaveable Interface in a List
    /// </summary>
    /// <param name="saveable">The class that implements ISaveable</param>
    public void Register(ISaveable saveable) {
        if (!saveables.Contains(saveable))
            saveables.Add(saveable);
    }

    /// <summary>
    /// Unregisters the classes that implement the ISaveable Interface in a List
    /// </summary>
    /// <param name="saveable">The class that implements ISaveable</param>
    public void Unregister(ISaveable saveable) {
        saveables.Remove(saveable);
    }

    /// <summary>
    /// Executes all Save Functions for every object in the saveables Array and puts it in a json file
    /// </summary>
    public void SaveGame() {
        List<ObjectWrapper> objects = new();

        foreach (var s in saveables) {
            object data = s.Save();

            objects.Add(new ObjectWrapper {
                id = s.GetSaveID(),
                type = data.GetType().AssemblyQualifiedName,
                json = JsonUtility.ToJson(data)
            });
        }

        SaveWrapper wrapper = new SaveWrapper { objects = objects };
        File.WriteAllText(savePath, JsonUtility.ToJson(wrapper, true));

        //Debug.Log("Saved!");
    }

    /// <summary>
    /// Loads everything from the save.json file
    /// </summary>
    public void LoadGame() {
        if (!File.Exists(savePath)) {
            Debug.LogWarning("No Save File found!");
            return;
        }

        SaveWrapper wrapper = JsonUtility.FromJson<SaveWrapper>(File.ReadAllText(savePath));

        foreach (var s in saveables) {
            ObjectWrapper data = wrapper.objects.Find(o => o.id == s.GetSaveID());

            if (data == null) {
                continue;
            }

            Type type = Type.GetType(data.type);

            object obj = JsonUtility.FromJson(data.json, type);

            s.Load(obj);
        }

        //Debug.Log("Loaded!");
    }

    /// <summary>
    /// Saves any Data without implementing the ISaveable Interface
    /// </summary>
    /// <typeparam name="T">The Type of the Data that should be saved</typeparam>
    /// <param name="id">The id of the data</param>
    /// <param name="data">the data itself</param>
    public void SaveData<T>(string id, T data) {
        string path = Path.Combine(Application.persistentDataPath, $"{id}.json");

        string json = JsonUtility.ToJson(data, true);

        File.WriteAllText(path, json);

        //Debug.Log("Saved!");
    }

    /// <summary>
    /// Loads any Data without implementing the ISaveable Interface
    /// </summary>
    /// <typeparam name="T">The Type of the Data that should be loaded</typeparam>
    /// <param name="id">The id of the data</param>
    /// <returns>The json as a string</returns>
    public T LoadData<T>(string id) {
        string path = Path.Combine(Application.persistentDataPath, $"{id}.json");

        if (!File.Exists(path))
            return default;

        string json = File.ReadAllText(path);

        //Debug.Log("Loaded!");

        return JsonUtility.FromJson<T>(json);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="id"></param>
    /// <returns>The </returns>
    public object LoadDataFromSave(string id) {
        string path = Path.Combine(Application.persistentDataPath, savePath);

        if (!File.Exists(path))
            return null;

        SaveWrapper wrapper = JsonUtility.FromJson<SaveWrapper>(File.ReadAllText(path));


        ObjectWrapper data = wrapper.objects.Find(o => o.id == id);

        if (data == null) {
            return null;
        }

        Type type = Type.GetType(data.type);

        object obj = JsonUtility.FromJson(data.json, type);

        //Debug.Log("Loaded!");

        return obj;
    }

    /// <summary>
    /// The Object Wrapper class which is the base of how the objects get displayed in the JSON
    /// </summary>
    [Serializable]
    private class ObjectWrapper {
        public string id;
        public string type;
        public string json;
    }

    /// <summary>
    /// The Save Wrapper class which has all the Objects in a List
    /// </summary>
    [Serializable]
    private class SaveWrapper {
        public List<ObjectWrapper> objects;
    }
}
