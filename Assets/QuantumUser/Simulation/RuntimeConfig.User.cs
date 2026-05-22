using Photon.Deterministic;

namespace Quantum {
  partial class RuntimeConfig {
    public AssetRef<ConfigAssets> ConfigAssets;
    public AssetRef<EntityPrototype> PrototypeRef;


    partial void SerializeUserData(BitStream stream)
    {
      stream.Serialize(ref ConfigAssets.Id);
      stream.Serialize(ref PrototypeRef.Id);
    }
  }
}