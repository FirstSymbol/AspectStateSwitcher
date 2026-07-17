using System;
using System.Collections.Generic;
using UnityEngine;

namespace AspectSwitcher
{
    [Serializable]
    public class TransformSnapshotEntry : SnapshotEntry<TransformData>
    {
        [field: SerializeField] public override TransformData data { get; set; } = new();
    }
    [AddComponentMenu("Aspect Switcher/Snapshots/Transform Snapshot")]
    public sealed class TransformSnapshot : AspectSnapshot<TransformData, TransformSnapshotEntry>
    {
        protected override Component FindDefaultTarget()                  => transform;
    }
}
