using System;
using AspectSwitcher;
using UnityEngine;

namespace UnityEngine.UI.Snapshots
{
    [Serializable]
    public class UnityEventSnapshotEntry : SnapshotEntry<UnityEventSnapshotData> { }

    public class UnityEventSnapshot : AspectSnapshot<UnityEventSnapshotData, UnityEventSnapshotEntry>
    {
        protected override Component FindDefaultTarget() => transform;
    }
}
