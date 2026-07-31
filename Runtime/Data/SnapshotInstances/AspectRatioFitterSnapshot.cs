using System;
using AspectSwitcher;
using UnityEngine;
using UnityEngine.UI;

namespace UnityEngine.UI.Snapshots
{
    [Serializable]
    public class AspectRatioFitterSnapshotEntry : SnapshotEntry<AspectRatioFitterSnapshotData> { }

    public class AspectRatioFitterSnapshot : AspectSnapshot<AspectRatioFitterSnapshotData, AspectRatioFitterSnapshotEntry>
    {
        protected override Component FindDefaultTarget() => transform;

        private void OnValidate()
        {
            if (target is AspectRatioFitter fitter)
                target = fitter.transform;
        }
    }
}
