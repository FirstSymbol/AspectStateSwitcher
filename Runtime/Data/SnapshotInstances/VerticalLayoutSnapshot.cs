using System;
using AspectSwitcher;
using UnityEngine;
using UnityEngine.UI;

namespace UnityEngine.UI.Snapshots
{
    [Serializable]
    public class VerticalLayoutSnapshotEntry : SnapshotEntry<VerticalLayoutSnapshotData> { }

    public class VerticalLayoutSnapshot : AspectSnapshot<VerticalLayoutSnapshotData, VerticalLayoutSnapshotEntry>
    {
        protected override Component FindDefaultTarget() => transform;

        private void OnValidate()
        {
            if (target is VerticalLayoutGroup vertical)
                target = vertical.transform;
        }
    }
}
