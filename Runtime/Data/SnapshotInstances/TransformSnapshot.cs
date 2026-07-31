using System;
using UnityEngine;

namespace AspectSwitcher
{
    [Serializable]
    public class TransformSnapshotEntry : SnapshotEntry<TransformData> { }

    [AddComponentMenu("Aspect Switcher/Snapshots/Transform Snapshot")]
    public sealed class TransformSnapshot : AspectSnapshot<TransformData, TransformSnapshotEntry>
    {
        protected override Component FindDefaultTarget() => transform;
    }
}