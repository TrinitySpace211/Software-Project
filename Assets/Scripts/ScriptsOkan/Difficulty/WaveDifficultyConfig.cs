using System;

[Serializable]
public struct WaveDifficultyConfig {
    public int baseZombieCount;
    public int zombiesPerDay;
    public int maxZombiesTotal;
    public int sprinterStartDay;
    public int maxSprinters;
    public int tankStartDay;
    public int maxTanksTotal;
}