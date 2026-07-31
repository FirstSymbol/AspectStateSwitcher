using System;
using AspectSwitcher;
using UnityEngine;
using UnityEngine.UI;

namespace UnityEngine.UI.Snapshots
{
    /// <summary>
    /// Snapshot data for ContentSizeFitter component — create with params or remove.
    /// </summary>
    [Serializable]
    public class ContentSizeFitterSnapshotData : AspectSwitcher.ComponentSnapshotData<ContentSizeFitter>
    {

        public ContentSizeFitter.FitMode horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        public ContentSizeFitter.FitMode verticalFit = ContentSizeFitter.FitMode.Unconstrained;

        public override void CaptureFrom(Component target)
        {
            if (target == null) return;

            var fitter = target.gameObject.GetComponent<ContentSizeFitter>();
            if (fitter != null)
            {
                action = LayoutAction.Update;
                horizontalFit = fitter.horizontalFit;
                verticalFit = fitter.verticalFit;
            }
            else
            {
                action = LayoutAction.Remove;
            }
        }

        protected override void CreateComponent(Component target, SnapshotData previousStateData, float t)
        {
            if (t < 1f) return;
            var fitter = target.gameObject.AddComponent<ContentSizeFitter>();
            UpdateComponent(target, previousStateData, t, fitter);
        }

        protected override void UpdateComponent(Component target, SnapshotData previousStateData, float t, ContentSizeFitter component)
        {
            if (t < 1f) return;
            component.horizontalFit = horizontalFit;
            component.verticalFit = verticalFit;
        }
    }
}
