using System;
using AspectSwitcher;
using UnityEngine;
using UnityEngine.UI;

namespace UnityEngine.UI.Snapshots
{
    [Serializable]
    public class CanvasScalerSnapshotEntry : SnapshotEntry<CanvasScalerSnapshotData> { }

    public class CanvasScalerSnapshot : AspectSnapshot<CanvasScalerSnapshotData, CanvasScalerSnapshotEntry>
    {
        protected override Component FindDefaultTarget() => transform;

        private void OnValidate()
        {
            if (target is CanvasScaler scaler)
                target = scaler.transform;
        }
    }
}
