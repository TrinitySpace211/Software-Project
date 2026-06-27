
public interface ISaveable {

    string GetSaveID();
    object Save();
    void Load(object data);
}
