using System;
using AspectSwitcher;
using UnityEngine;
using UnityEngine.UI;

namespace UnityEngine.UI.Snapshots
{
    /// <summary>
    /// Snapshot data for LayoutElement — create with params or remove.
    /// Useful for overriding min/preferred/flexible sizes per aspect ratio.
    /// </summary>
    [Serializable]
    public class LayoutElementSnapshotData : SnapshotData
    {
        public LayoutAction action = LayoutAction.Update;

        public bool ignoreLayout;
        public float minWidth = -1;
        public float minHeight = -1;
        public float preferredWidth = -1;
        public float preferredHeight = -1;
        public float flexibleWidth = -1;
        public float flexibleHeight = -1;
        public int layoutPriority = 1;

        public override void CaptureFrom(Component target)
        {
            if (target == null) return;

            var le = target.gameObject.GetComponent<LayoutElement>();
            if (le != null)
            {
                action = LayoutAction.Create;
                ignoreLayout = le.ignoreLayout;
                minWidth = le.minWidth;
                minHeight = le.minHeight;
                preferredWidth = le.preferredWidth;
                preferredHeight = le.preferredHeight;
                flexibleWidth = le.flexibleWidth;
                flexibleHeight = le.flexibleHeight;
                layoutPriority = le.layoutPriority;
            }
            else
            {
                action = LayoutAction.Remove;
            }
        }

        public override void ApplyTo(Component target, SnapshotData previousStateData, float t)
        {
            if (target == null) return;
            if (t < 1f) return;

            var go = target.gameObject;

            if (action == LayoutAction.Remove)
            {
                var existing = go.GetComponent<LayoutElement>();
                if (existing != null)
                    Object.DestroyImmediate(existing);
                return;
            }

            var le = go.GetComponent<LayoutElement>();

            if (le == null && action != LayoutAction.Update)
                le = go.AddComponent<LayoutElement>();

            if (le == null) return;

            le.ignoreLayout = ignoreLayout;
            le.minWidth = minWidth;
            le.minHeight = minHeight;
            le.preferredWidth = preferredWidth;
            le.preferredHeight = preferredHeight;
            le.flexibleWidth = flexibleWidth;
            le.flexibleHeight = flexibleHeight;
            le.layoutPriority = layoutPriority;
        }
    }
}
