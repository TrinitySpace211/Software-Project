using UnityEngine;

public enum StatType { Health, Armor }

public class PlayerStats {

    readonly StatsMediator mediator;
    readonly BaseStats baseStats;

    private int currentHealth;

    public StatsMediator Mediator => mediator;

    // Max HP
    public int MaxHealth {
        get {
            var q = new Query(StatType.Health, baseStats.health);
            mediator.PerformQuery(this, q);
            return q.Value;
        }
    }

    // Aktuelle HP
    public int CurrentHealth => currentHealth;

    public int Armor {
        get {
            var q = new Query(StatType.Armor, baseStats.armor);
            mediator.PerformQuery(this, q);
            return q.Value;
        }
    }

    public PlayerStats(
        StatsMediator mediator,
        BaseStats baseStats
    ) {
        this.mediator = mediator;
        this.baseStats = baseStats;

        // Spieler startet mit voller HP
        currentHealth = MaxHealth;
    }

    public void ChangeHealth(int amount) {
        currentHealth += amount;

        currentHealth = Mathf.Clamp(
            currentHealth,
            0,
            MaxHealth
        );
    }

    public override string ToString() =>
        $"Health: {CurrentHealth}/{MaxHealth}, Armor: {Armor}";
}