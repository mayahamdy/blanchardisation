namespace Quantum {
  unsafe partial class Frame {
    public Input GetPlayerInputValue(PlayerRef player)
    {
      return *GetPlayerInput(player);
    }
  }
}
