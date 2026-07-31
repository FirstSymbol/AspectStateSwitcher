using System;
using AspectSwitcher;
using UnityEngine;
using UnityEngine.UI;

namespace UnityEngine.UI.Snapshots
{
    [Serializable]
    public class LayoutElementSnapshotEntry : SnapshotEntry<LayoutElementSnapshotData> { }

    public class LayoutElementSnapshot : AspectSnapshot<LayoutElementSnapshotData, LayoutElementSnapshotEntry>
    {
        protected override Component FindDefaultTarget() => transform;

        private void OnValidate()
        {
            if (target is LayoutElement le)
                target = le.transform;
        }
    }
}
