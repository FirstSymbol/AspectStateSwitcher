using System;
using AspectSwitcher;
using UnityEngine;

namespace UnityEngine.UI.Snapshots
{
    [Serializable]
    public class DestroyGameObjectSnapshotData : SnapshotData
    {
        [Tooltip("If true, the target component will be destroyed when this state is applied.")]
        public bool destroy = true;

        public override void CaptureFrom(Component target)
        {
            destroy = target == null;
        }

        public override void ApplyTo(Component target, SnapshotData previousStateData, float t)
        {
            if (target == null) return;
            if (t < 1f) return;

            if (destroy)
            {
                if (Application.isPlaying)
                    Object.Destroy(target);
                else
                    Object.DestroyImmediate(target);
            }
        }
    }
}
