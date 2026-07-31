using System;
using AspectSwitcher;
using UnityEngine;

namespace UnityEngine.UI.Snapshots
{
    [Serializable]
    public class CanvasSnapshotEntry : SnapshotEntry<CanvasSnapshotData> { }

    public class CanvasSnapshot : AspectSnapshot<CanvasSnapshotData, CanvasSnapshotEntry>
    {
        protected override Component FindDefaultTarget() => transform;

        private void OnValidate()
        {
            if (target is Canvas canvas)
                target = canvas.transform;
        }
    }
}
