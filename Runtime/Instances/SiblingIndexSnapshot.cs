using System;
using AspectSwitcher;
using UnityEngine;

namespace UnityEngine.UI.Snapshots
{
    [Serializable]
    public class SiblingIndexSnapshotEntry : SnapshotEntry<SiblingIndexSnapshotData> { }

    public class SiblingIndexSnapshot : AspectSnapshot<SiblingIndexSnapshotData, SiblingIndexSnapshotEntry>
    {
        protected override Component FindDefaultTarget() => transform;
    }
}
