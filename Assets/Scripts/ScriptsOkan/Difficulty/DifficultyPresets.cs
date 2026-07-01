public static class DifficultyPresets {
    public static readonly WaveDifficultyConfig Easy = new() {
        baseZombieCount = 8,
        zombiesPerDay = 3,
        maxZombiesTotal = 40,
        sprinterStartDay = 3,
        maxSprinters = 5,
        tankStartDay = 7,
        maxTanksTotal = 2
    };

    public static readonly WaveDifficultyConfig Medium = new() {
        baseZombieCount = 15,
        zombiesPerDay = 5,
        maxZombiesTotal = 60,
        sprinterStartDay = 2,
        maxSprinters = 8,
        tankStartDay = 5,
        maxTanksTotal = 4
    };

    public static readonly WaveDifficultyConfig Hard = new() {
        baseZombieCount = 20,
        zombiesPerDay = 7,
        maxZombiesTotal = 90,
        sprinterStartDay = 1,
        maxSprinters = 12,
        tankStartDay = 4,
        maxTanksTotal = 6
    };

    public static WaveDifficultyConfig Get(DifficultyLevel level) {
        return level switch {
            DifficultyLevel.Easy => Easy,
            DifficultyLevel.Medium => Medium,
            DifficultyLevel.Hard => Hard,
            _ => Medium
        };
    }
}