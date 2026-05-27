using System;
using System.Collections.Generic;
using UnityEngine;

namespace AspectSwitcher
{
    [AddComponentMenu("Aspect Switcher/Snapshots/UI Transform Snapshot")]
    public sealed class UITransformSnapshot : AspectSnapshot
    {
        [Serializable]
        public class Entry
        {
            public AspectState     state = AspectState.Portrait;
            public UITransformData data  = new UITransformData();
        }

        public List<Entry> entries = new List<Entry>();

        public override ISnapshotData CreateSnapshotData()             => new UITransformData();
        public override ISnapshotData GetDataAt(int i)                  => entries[i].data;
        protected override ISnapshotData FindDataForState(AspectState s) => entries.Find(e => e.state == s)?.data;
        protected override Component FindDefaultTarget()                 => GetComponent<RectTransform>();
    }
}
