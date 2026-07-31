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
    public class LayoutElementSnapshotData : AspectSwitcher.ComponentSnapshotData<LayoutElement>
    {

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
                action = LayoutAction.Update;
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

        protected override void CreateComponent(Component target, SnapshotData previousStateData, float t)
        {
            if (t < 1f) return;
            var le = target.gameObject.AddComponent<LayoutElement>();
            UpdateComponent(target, previousStateData, t, le);
        }

        protected override void UpdateComponent(Component target, SnapshotData previousStateData, float t, LayoutElement component)
        {
            if (t < 1f) return;
            component.ignoreLayout = ignoreLayout;
            component.minWidth = minWidth;
            component.minHeight = minHeight;
            component.preferredWidth = preferredWidth;
            component.preferredHeight = preferredHeight;
            component.flexibleWidth = flexibleWidth;
            component.flexibleHeight = flexibleHeight;
            component.layoutPriority = layoutPriority;
        }
    }
}
