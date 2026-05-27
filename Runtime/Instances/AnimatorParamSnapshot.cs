using System;
using System.Collections.Generic;
using UnityEngine;

namespace AspectSwitcher
{
    [AddComponentMenu("Aspect Switcher/Snapshots/Animator Param Snapshot")]
    public sealed class AnimatorParamSnapshot : AspectSnapshot
    {
        [Serializable]
        public class Entry
        {
            public AspectState      state = AspectState.Portrait;
            public AnimatorParamData data  = new AnimatorParamData();
        }

        public List<Entry> entries = new List<Entry>();

        public override ISnapshotData CreateSnapshotData()             => new AnimatorParamData();
        public override ISnapshotData GetDataAt(int i)                  => entries[i].data;
        protected override ISnapshotData FindDataForState(AspectState s) => entries.Find(e => e.state == s)?.data;
        protected override Component FindDefaultTarget()                 => GetComponent<Animator>();
    }
}
