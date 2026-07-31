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
    public class ContentSizeFitterSnapshotData : SnapshotData
    {
        public LayoutAction action = LayoutAction.Update;

        public ContentSizeFitter.FitMode horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        public ContentSizeFitter.FitMode verticalFit = ContentSizeFitter.FitMode.Unconstrained;

        public override void CaptureFrom(Component target)
        {
            if (target == null) return;

            var fitter = target.gameObject.GetComponent<ContentSizeFitter>();
            if (fitter != null)
            {
                action = LayoutAction.Create;
                horizontalFit = fitter.horizontalFit;
                verticalFit = fitter.verticalFit;
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
                var existing = go.GetComponent<ContentSizeFitter>();
                if (existing != null)
                    Object.DestroyImmediate(existing);
                return;
            }

            var fitter = go.GetComponent<ContentSizeFitter>();

            if (fitter == null && action != LayoutAction.Update)
                fitter = go.AddComponent<ContentSizeFitter>();

            if (fitter == null) return;

            fitter.horizontalFit = horizontalFit;
            fitter.verticalFit = verticalFit;
        }
    }
}
