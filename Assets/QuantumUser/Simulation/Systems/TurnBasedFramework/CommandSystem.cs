using Photon.Deterministic;
using UnityEngine;

namespace Quantum
{
    /// <summary>
    /// Reads player commands each tick and dispatches play or skip signals when the current turn is active.
    /// </summary>
    public unsafe class CommandSystem : SystemMainThread
    {
        /// <summary>
        /// Each tick, reads the current player's command and dispatches the appropriate play or skip signal if the turn is active.
        /// </summary>
        public override void Update(Frame frame)
        {
            TurnContainer* turnContainer = frame.Unsafe.GetPointerSingleton<TurnContainer>();
            TurnData currentTurn = turnContainer->CurrentTurn;
            if (currentTurn.Status != ETurnStatus.Active)
                return;

            if (currentTurn.Type == ETurnType.Waiting)
            {
                CheckStartCommand(frame, currentTurn);
                return;
            }

            PlayerRef currentPlayer = turnContainer->CurrentTurn.Player;
            switch (frame.GetPlayerCommand(currentPlayer))
            {
                case PlayCommand playCommand:
                    if (currentTurn.Type != ETurnType.Play)
                        return;

                    frame.Signals.OnPlayCommandReceived(currentPlayer, playCommand.Data);
                    frame.Events.PlayCommandReceived(currentPlayer, playCommand.Data);
                    break;

                case SkipCommand skipCommand:
                    TurnConfig config = frame.FindAsset<TurnConfig>(currentTurn.ConfigRef.Id);
                    if (config.IsSkippable == false)
                        return;

                    frame.Signals.OnSkipCommandReceived(currentPlayer, skipCommand.Data);
                    frame.Events.SkipCommandReceived(currentPlayer, skipCommand.Data);
                    break;
            }
        }

        /// <summary>
        /// During the initial Waiting turn, reads all players commands and dispatches the start signal.
        /// </summary>
        private void CheckStartCommand(Frame frame, TurnData currentTurn)
        {
            for (int playerIndex = 0; playerIndex < frame.MaxPlayerCount; playerIndex++)
            {
                DeterministicCommand playerCommand = frame.GetPlayerCommand(playerIndex);

                switch (frame.GetPlayerCommand(playerIndex))
                {
                    case StartCommand startCommand:
                        if (currentTurn.Type != ETurnType.Waiting)
                            return;

                        frame.Signals.OnStartCommandReceived(playerIndex);
                        break;
                }
            }
        }
    }
}