using System;
using System.Collections.Generic;
using UnityEngine;

namespace AspectSwitcher
{
    [AddComponentMenu("Aspect Switcher/Snapshots/Component Field Snapshot")]
    public sealed class ComponentFieldSnapshot : AspectSnapshot
    {
        [Serializable]
        public class Entry
        {
            public AspectState       state = AspectState.Portrait;
            public ComponentFieldData data  = new ComponentFieldData();
        }

        public List<Entry> entries = new List<Entry>();

        public override ISnapshotData CreateSnapshotData()             => new ComponentFieldData();
        public override ISnapshotData GetDataAt(int i)                  => entries[i].data;
        protected override ISnapshotData FindDataForState(AspectState s) => entries.Find(e => e.state == s)?.data;
    }
}
