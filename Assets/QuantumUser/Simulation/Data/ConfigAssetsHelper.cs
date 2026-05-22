namespace Quantum
{
    /// <summary>
    /// Utility class providing static accessors to retrieve typed config assets from the current frame's runtime configuration.
    /// </summary>
    public unsafe class ConfigAssetsHelper
    {
        /// <summary>
        /// Retrieves the GameConfig asset from the runtime configuration of the given frame.
        /// </summary>
        public static GameConfig GetGameConfig(Frame frame)
        {
            ConfigAssets configAssets = frame.FindAsset<ConfigAssets>(frame.RuntimeConfig.ConfigAssets.Id);
            return frame.FindAsset<GameConfig>(configAssets.GameConfig.Id);
        }

        /// <summary>
        /// Retrieves the start-countdown TurnConfig asset from the runtime configuration of the given frame.
        /// </summary>
        public static TurnConfig GetStartCountdownConfig(Frame frame)
        {
            ConfigAssets configAssets = frame.FindAsset<ConfigAssets>(frame.RuntimeConfig.ConfigAssets.Id);
            return frame.FindAsset<TurnConfig>(configAssets.StartCountdownConfig.Id);
        }
        
        /// <summary>
        /// Retrieves the Waiting TurnConfig asset from the runtime configuration of the given frame.
        /// </summary>
        public static TurnConfig GetWaitingConfig(Frame frame)
        {
            ConfigAssets configAssets = frame.FindAsset<ConfigAssets>(frame.RuntimeConfig.ConfigAssets.Id);
            return frame.FindAsset<TurnConfig>(configAssets.WaitingConfig.Id);
        }

        /// <summary>
        /// Retrieves the general countdown TurnConfig asset from the runtime configuration of the given frame.
        /// </summary>
        public static TurnConfig GetCountdownTurnConfig(Frame frame)
        {
            ConfigAssets configAssets = frame.FindAsset<ConfigAssets>(frame.RuntimeConfig.ConfigAssets.Id);
            return frame.FindAsset<TurnConfig>(configAssets.CountdownTurnConfig.Id);
        }

        /// <summary>
        /// Retrieves the play-phase TurnConfig asset from the runtime configuration of the given frame.
        /// </summary>
        public static TurnConfig GetPlayTurnConfig(Frame frame)
        {
            ConfigAssets configAssets = frame.FindAsset<ConfigAssets>(frame.RuntimeConfig.ConfigAssets.Id);
            return frame.FindAsset<TurnConfig>(configAssets.PlayTurnConfig.Id);
        }
    }
}