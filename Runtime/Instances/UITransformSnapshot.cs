using System;
using System.Collections.Generic;
using UnityEngine;

namespace AspectSwitcher
{
    [AddComponentMenu("Aspect Switcher/Snapshots/UI Transform Snapshot")]
    public sealed class UITransformSnapshot : AspectSnapshot
    {
        [Serializable]
        public class Entry : ISerializationCallbackReceiver
        {
            [HideInInspector] public AspectState state; // migrated to states on first load
            public List<AspectState> states = new List<AspectState>();
            public UITransformData   data   = new UITransformData();

            public void OnBeforeSerialize() { }
            public void OnAfterDeserialize()
            {
                if (states.Count == 0)
                    states.Add(state);
            }
        }

        public List<Entry> entries = new List<Entry>();

        public override ISnapshotData CreateSnapshotData()              => new UITransformData();
        public override ISnapshotData GetDataAt(int i)                   => entries[i].data;
        protected override ISnapshotData FindDataForState(AspectState s)
        {
            for (int i = 0; i < entries.Count; i++)
                if (entries[i].states.Contains(s)) return entries[i].data;
            return null;
        }
        protected override Component FindDefaultTarget()                  => GetComponent<RectTransform>();
    }
}
