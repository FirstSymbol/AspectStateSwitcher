using System;
using UnityEngine;
using UnityEngine.UI;

namespace AspectSwitcher
{
    [Serializable]
    public class LayoutGroupTypeSnapshotEntry : SnapshotEntry<LayoutGroupTypeData> { }

    public class LayoutGroupTypeSnapshot : AspectSnapshot<LayoutGroupTypeData, LayoutGroupTypeSnapshotEntry>
    {
        protected override Component FindDefaultTarget() => transform;

        private void OnValidate()
        {
            if (target is HorizontalLayoutGroup horizontal)
                target = horizontal.transform;
            else if (target is VerticalLayoutGroup vertical)
                target = vertical.transform;
        }
    }
}
