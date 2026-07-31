using System;
using UnityEngine;
using UnityEngine.UI;

namespace AspectSwitcher
{
    [Serializable]
    public class GridLayoutGroupSnapshotEntry : SnapshotEntry<GridLayoutGroupData> { }

    [AddComponentMenu("Aspect Switcher/Snapshots/Grid Layout Group Snapshot")]
    public sealed class GridLayoutGroupSnapshot : AspectSnapshot<GridLayoutGroupData, GridLayoutGroupSnapshotEntry>
    {
        protected override Component FindDefaultTarget() => GetComponent<GridLayoutGroup>();
    }
}
