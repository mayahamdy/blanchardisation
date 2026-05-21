namespace Quantum
{
    /// <summary>
    /// Ticks the current turn's timer each simulation frame and triggers a turn-end signal when time runs out.
    /// </summary>
    public unsafe class TurnTimerSystem : SystemMainThread
    {
        /// <summary>
        /// Creates the TurnContainer singleton.
        /// </summary>
        public override void OnInit(Frame frame)
        {
            frame.Unsafe.GetOrAddSingletonPointer<TurnContainer>();
        }
        
        /// <summary>
        /// Ticks the current turn's timer every simulation frame.
        /// </summary>
        public override void Update(Frame frame)
        {
            TurnContainer* turnContainer = frame.Unsafe.GetPointerSingleton<TurnContainer>();
            turnContainer->CurrentTurn.Update(frame);
        }
    }
}