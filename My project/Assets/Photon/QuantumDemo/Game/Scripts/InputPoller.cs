using Photon.Deterministic;
using Quantum;
using UnityEngine;
using Input = Quantum.Input;

public sealed class InputPoller : MonoBehaviour
{
  public static InputPoller Instance { get; private set; }
  public Input RegisteredInput;
  
  private void Awake()
  {
    if (Instance != null && Instance != this)
    {
      Destroy(this);
    }
    else
    {
      Instance = this;
    }
  }
  
  private void OnEnable() {
    QuantumCallback.Subscribe(this, (CallbackPollInput callback) => PollInput(callback));
    RegisteredInput = default(Input);
  }
  
  public void PollInput(CallbackPollInput callback) {
    callback.SetInput(RegisteredInput, DeterministicInputFlags.Repeatable);
  }


  public void RegisterInput(FPVector3 dir, FP forceBarMarkPos)
  {
    RegisteredInput.Direction = dir;
    RegisteredInput.ForceBarMarkPos = forceBarMarkPos;
  }

  public void ResetRegisteredInput(int player)
  {
      RegisteredInput = default(Input);
  }
}