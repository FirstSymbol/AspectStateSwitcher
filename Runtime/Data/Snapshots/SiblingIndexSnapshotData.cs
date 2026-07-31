using System;
using AspectSwitcher;
using UnityEngine;

namespace UnityEngine.UI.Snapshots
{
    /// <summary>
    /// Controls the sibling index (order) of the target transform per aspect state.
    /// Useful when you need to reorder children in a layout for different resolutions.
    /// </summary>
    [Serializable]
    public class SiblingIndexSnapshotData : SnapshotData
    {
        [Tooltip("Sibling index to set. Use -1 to set as last sibling.")]
        public int siblingIndex;

        public override void CaptureFrom(Component target)
        {
            if (target == null) return;
            siblingIndex = target.transform.GetSiblingIndex();
        }

        public override void ApplyTo(Component target, SnapshotData previousStateData, float t)
        {
            if (target == null) return;
            if (t < 1f) return;

            if (siblingIndex < 0)
                target.transform.SetAsLastSibling();
            else
                target.transform.SetSiblingIndex(siblingIndex);
        }
    }
}
