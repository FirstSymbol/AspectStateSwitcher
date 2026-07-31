using System;
using UnityEngine;
using UnityEngine.UI;

namespace AspectSwitcher
{
    [Serializable]
    public class HorizontalOrVerticalLayoutGroupSnapshotEntry : SnapshotEntry<HorizontalOrVerticalLayoutGroupData> { }
    public class HorizontalOrVerticalLayoutGroupSnapshot : AspectSnapshot<HorizontalOrVerticalLayoutGroupData,  HorizontalOrVerticalLayoutGroupSnapshotEntry>
    {
        protected override Component FindDefaultTarget()
        {
            if (TryGetComponent(out HorizontalOrVerticalLayoutGroup layout))
            {
                return layout;
            }
            return null;
        }
    }
}