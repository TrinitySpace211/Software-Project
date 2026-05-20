using UnityEngine;

/// <summary>
///     Enthält alle Basiswerte eines Enemy-Typs als ScriptableObject.
///     Erstelle für jeden Zombie-Typ ein eigenes Asset via Rechtsklick → Create → Enemy → Stats.
/// </summary>
[CreateAssetMenu(fileName = "NewEnemyStats", menuName = "Enemy/Stats")]
public class EnemyStatsSO : ScriptableObject {
    public int maxHealth = 100;
    public float moveSpeed = 1f;
    public int damage = 10;
    public float attackRange = 1.5f;
    public float attackCooldown = 1f;
}