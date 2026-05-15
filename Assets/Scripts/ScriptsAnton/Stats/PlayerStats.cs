
public enum StatType { Health, Armor }

public class PlayerStats {
    readonly StatsMediator mediator;
    readonly BaseStats baseStats;

    public StatsMediator Mediator => mediator;

    public int Health {
        get {
            var q = new Query(StatType.Health, baseStats.health);
            mediator.PerformQuery(this, q);
            return q.Value;
        }
    }

    public int Armor {
        get {
            var q = new Query(StatType.Armor, baseStats.armor);
            mediator.PerformQuery(this, q);
            return q.Value;
        }
    }

    public PlayerStats(StatsMediator mediator, BaseStats baseStats) {
        this.mediator = mediator;
        this.baseStats = baseStats;
    }

    public override string ToString() => $"Health: {Health}, Armor: {Armor}";
}
