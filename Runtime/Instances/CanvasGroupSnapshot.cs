using System;
using System.Collections.Generic;
using UnityEngine;

namespace AspectSwitcher
{
    [AddComponentMenu("Aspect Switcher/Snapshots/Canvas Group Snapshot")]
    public sealed class CanvasGroupSnapshot : AspectSnapshot
    {
        [Serializable]
        public class Entry
        {
            public AspectState     state = AspectState.Portrait;
            public CanvasGroupData data  = new CanvasGroupData();
        }

        public List<Entry> entries = new List<Entry>();

        public override ISnapshotData CreateSnapshotData()             => new CanvasGroupData();
        public override ISnapshotData GetDataAt(int i)                  => entries[i].data;
        protected override ISnapshotData FindDataForState(AspectState s) => entries.Find(e => e.state == s)?.data;
        protected override Component FindDefaultTarget()                 => GetComponent<CanvasGroup>();
    }
}
