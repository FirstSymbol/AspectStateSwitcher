using System;
using System.Collections.Generic;
using UnityEngine;

namespace AspectSwitcher
{
    [AddComponentMenu("Aspect Switcher/Snapshots/Transform Snapshot")]
    public sealed class TransformSnapshot : AspectSnapshot
    {
        [Serializable]
        public class Entry
        {
            public AspectState  state = AspectState.Portrait;
            public TransformData data = new TransformData();
        }

        public List<Entry> entries = new List<Entry>();

        public override ISnapshotData CreateSnapshotData()             => new TransformData();
        public override ISnapshotData GetDataAt(int i)                  => entries[i].data;
        protected override ISnapshotData FindDataForState(AspectState s) => entries.Find(e => e.state == s)?.data;
        protected override Component FindDefaultTarget()                 => transform;
    }
}
