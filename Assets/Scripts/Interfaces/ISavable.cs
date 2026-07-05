
/// <summary>
/// Interface to determine which scripts can be saved
/// </summary>
public interface ISaveable {

    string GetSaveID();
    object Save();
    void Load(object data);
}
