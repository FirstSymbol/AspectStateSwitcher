using System;
using System.Collections.Generic;
using UnityEngine;

namespace AspectSwitcher
{
    [Serializable]
    public class UITransformSnapshotEntry : SnapshotEntry<UITransformData>
    {
        public override UITransformData data { get; set; }
    }
    
    [AddComponentMenu("Aspect Switcher/Snapshots/UI Transform Snapshot")]
    public sealed class UITransformSnapshot : AspectSnapshot<UITransformData, UITransformSnapshotEntry>
    {
        protected override Component FindDefaultTarget()                  => GetComponent<RectTransform>();
    }
}
