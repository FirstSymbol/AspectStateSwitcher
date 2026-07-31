using System;
using AspectSwitcher;
using UnityEngine;

namespace UnityEngine.UI.Snapshots
{
    [Serializable]
    public class ReparentSnapshotEntry : SnapshotEntry<ReparentSnapshotData> { }

    public class ReparentSnapshot : AspectSnapshot<ReparentSnapshotData, ReparentSnapshotEntry>
    {
        protected override Component FindDefaultTarget() => transform;
    }
}
