using UnityEngine;

/// <summary>
/// BaseStats of the PlayerStats as a Scriptable Object
/// </summary>
[CreateAssetMenu(fileName = "BaseStats", menuName = "Stats/BaseStats")]
public class BaseStats : ScriptableObject {

    public int health = 100;
    public int armor = 5;
}
