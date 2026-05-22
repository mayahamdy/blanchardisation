namespace Quantum
{
    /// <summary>
    /// Asset acting as a registry of all configuration asset references used by the simulation.
    /// </summary>
    public class ConfigAssets : AssetObject
    {
        public AssetRef<GameConfig> GameConfig;
        public AssetRef<TurnConfig> WaitingConfig;
        public AssetRef<TurnConfig> StartCountdownConfig;
        public AssetRef<TurnConfig> PlayTurnConfig;
        public AssetRef<TurnConfig> CountdownTurnConfig;
    }
}