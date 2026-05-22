#if QUANTUM_ENABLE_MIGRATION

using System;
using Quantum;

[Obsolete("Use " + nameof(QuantumEntityPrototypeConverter) + " instead.")]
public sealed unsafe partial class EntityPrototypeConverter : QuantumEntityPrototypeConverter {
  public EntityPrototypeConverter(QuantumMapData map, QuantumEntityPrototype[] orderedMapPrototypes) : base(map, orderedMapPrototypes) {
  }
  public EntityPrototypeConverter(QuantumEntityPrototype prototypeAsset) : base(prototypeAsset) {
  }
}

namespace Quantum {
  partial class QuantumEntityPrototypeConverter {
    [Obsolete]
    public void Convert(global::EntityPrototype prototype, out MapEntityId result) {
      if (AssetPrototype != null) {
        result = AssetPrototype == prototype ? MapEntityId.Create(0) : MapEntityId.Invalid;
      } else {
        var index = Array.IndexOf(OrderedMapPrototypes, prototype);
        result = index >= 0 ? MapEntityId.Create(index) : MapEntityId.Invalid;
      }
    }
  }
}
#endif