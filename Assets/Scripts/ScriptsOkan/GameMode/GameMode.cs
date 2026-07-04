/// <summary>
///     Global ausgewaehlter Spielmodus. Wird - wie <see cref="GameDifficulty" /> -
///     vor dem Spielstart per Menue-Button gesetzt und danach von WaveManager,
///     Inventory und NPCDialog ausgewertet.
/// </summary>
public static class GameMode {
    public static GameModeType Selected = GameModeType.None;
}
