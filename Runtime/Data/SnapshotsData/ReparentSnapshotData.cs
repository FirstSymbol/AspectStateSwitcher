using System;
using AspectSwitcher;
using UnityEngine;

namespace UnityEngine.UI.Snapshots
{
    /// <summary>
    /// Reparents the target transform to a different parent per aspect state.
    /// Useful for major layout restructuring between resolutions.
    /// </summary>
    [Serializable]
    public class ReparentSnapshotData : SnapshotData
    {
        [Tooltip("The new parent transform. If null, the object will be unparented.")]
        public Transform newParent;

        [Tooltip("If true, the world position will be preserved when reparenting.")]
        public bool worldPositionStays;

        [Tooltip("Sibling index to set after reparenting. Use -1 to keep default.")]
        public int siblingIndex = -1;

        public override void CaptureFrom(Component target)
        {
            if (target == null) return;
            newParent = target.transform.parent;
            siblingIndex = target.transform.GetSiblingIndex();
        }

        public override void ApplyTo(Component target, SnapshotData previousStateData, float t)
        {
            if (target == null) return;
            if (t < 1f) return;

            target.transform.SetParent(newParent, worldPositionStays);

            if (siblingIndex >= 0)
                target.transform.SetSiblingIndex(siblingIndex);
        }
    }
}
