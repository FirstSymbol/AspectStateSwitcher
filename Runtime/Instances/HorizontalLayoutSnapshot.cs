using System;
using AspectSwitcher;
using UnityEngine;
using UnityEngine.UI;

namespace UnityEngine.UI.Snapshots
{
    [Serializable]
    public class HorizontalLayoutSnapshotEntry : SnapshotEntry<HorizontalLayoutSnapshotData> { }

    public class HorizontalLayoutSnapshot : AspectSnapshot<HorizontalLayoutSnapshotData, HorizontalLayoutSnapshotEntry>
    {
        protected override Component FindDefaultTarget() => transform;

        private void OnValidate()
        {
            if (target is HorizontalLayoutGroup horizontal)
                target = horizontal.transform;
        }
    }
}
