using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SaveManager : MonoBehaviour {
    public static SaveManager Instance { get; private set; }

    [SerializeField] private SaveableRef[] saveables;
    private string savePath;

    private void OnValidate() {
        if (saveables == null) return;
        for (int i = 0; i < saveables.Length; i++) {
            saveables[i].OnValidate();
        }
    }

    private void Awake() {
        Instance = this;

        savePath = Path.Combine(Application.persistentDataPath, "save.json");
    }

    public void SaveGame() {
        List<string> jsonList = new();

        foreach (var s in saveables) {
            string json = JsonUtility.ToJson(new ObjectWrapper { json = JsonUtility.ToJson(s.Interface.Save()) });
            jsonList.Add(json);
        }

        SaveWrapper wrapper = new SaveWrapper { objects = jsonList };
        string finalJson = JsonUtility.ToJson(wrapper, true);
        File.WriteAllText(savePath, finalJson);

        //Debug.Log("Saved!");
    }

    public void LoadGame() {
        if (!File.Exists(savePath)) {
            Debug.LogWarning("No Save File found!");
            return;
        }

        string finalJson = File.ReadAllText(savePath);
        SaveWrapper wrapper = JsonUtility.FromJson<SaveWrapper>(finalJson);

        if (wrapper.objects.Count != saveables.Length) {
            Debug.LogWarning("Mismatch between saved objects and scene objects");
        }

        for (int i = 0; i < saveables.Length; i++) {
            string objectJson = wrapper.objects[i];
            ObjectWrapper wrapperObject = JsonUtility.FromJson<ObjectWrapper>(objectJson);

            saveables[i].Interface.Load(JsonUtility.FromJson(wrapperObject.json, saveables[i].Interface.Save().GetType()));
        }
        //Debug.Log("Loaded!");
    }

    [System.Serializable]
    private class ObjectWrapper {
        public string json;
    }

    [System.Serializable]
    private class SaveWrapper {
        public List<string> objects;
    }
}
