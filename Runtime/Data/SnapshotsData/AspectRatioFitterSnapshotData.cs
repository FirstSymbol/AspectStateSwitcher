using System;
using AspectSwitcher;
using UnityEngine;
using UnityEngine.UI;

namespace UnityEngine.UI.Snapshots
{
    /// <summary>
    /// Snapshot data for AspectRatioFitter — create with params or remove.
    /// </summary>
    [Serializable]
    public class AspectRatioFitterSnapshotData : SnapshotData
    {
        public LayoutAction action = LayoutAction.Update;

        public AspectRatioFitter.AspectMode aspectMode = AspectRatioFitter.AspectMode.None;
        public float aspectRatio = 1f;

        public override void CaptureFrom(Component target)
        {
            if (target == null) return;

            var fitter = target.gameObject.GetComponent<AspectRatioFitter>();
            if (fitter != null)
            {
                action = LayoutAction.Create;
                aspectMode = fitter.aspectMode;
                aspectRatio = fitter.aspectRatio;
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
                var existing = go.GetComponent<AspectRatioFitter>();
                if (existing != null)
                    Object.DestroyImmediate(existing);
                return;
            }

            var fitter = go.GetComponent<AspectRatioFitter>();

            if (fitter == null && action != LayoutAction.Update)
                fitter = go.AddComponent<AspectRatioFitter>();

            if (fitter == null) return;

            fitter.aspectMode = aspectMode;
            fitter.aspectRatio = aspectRatio;
        }
    }
}
