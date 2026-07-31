using System;
using AspectSwitcher;
using UnityEngine;
using UnityEngine.UI;

namespace UnityEngine.UI.Snapshots
{
    [Serializable]
    public class ContentSizeFitterSnapshotEntry : SnapshotEntry<ContentSizeFitterSnapshotData> { }

    public class ContentSizeFitterSnapshot : AspectSnapshot<ContentSizeFitterSnapshotData, ContentSizeFitterSnapshotEntry>
    {
        protected override Component FindDefaultTarget() => transform;

        private void OnValidate()
        {
            if (target is ContentSizeFitter fitter)
                target = fitter.transform;
        }
    }
}
