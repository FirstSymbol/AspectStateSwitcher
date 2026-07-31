using System;
using AspectSwitcher;
using UnityEngine;
using UnityEngine.Events;

namespace UnityEngine.UI.Snapshots
{
    [Serializable]
    public class UnityEventSnapshotData : SnapshotData
    {
        public UnityEvent onStateApplied;

        public override void CaptureFrom(Component target) { }

        public override void ApplyTo(Component target, SnapshotData previousStateData, float t)
        {
            if (t < 1f) return;
            onStateApplied?.Invoke();
        }
    }
}
