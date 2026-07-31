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
    public class AspectRatioFitterSnapshotData : AspectSwitcher.ComponentSnapshotData<AspectRatioFitter>
    {

        public AspectRatioFitter.AspectMode aspectMode = AspectRatioFitter.AspectMode.None;
        public float aspectRatio = 1f;

        public override void CaptureFrom(Component target)
        {
            if (target == null) return;

            var fitter = target.gameObject.GetComponent<AspectRatioFitter>();
            if (fitter != null)
            {
                action = LayoutAction.Update;
                aspectMode = fitter.aspectMode;
                aspectRatio = fitter.aspectRatio;
            }
            else
            {
                action = LayoutAction.Remove;
            }
        }

        protected override void CreateComponent(Component target, SnapshotData previousStateData, float t)
        {
            if (t < 1f) return;
            var fitter = target.gameObject.AddComponent<AspectRatioFitter>();
            UpdateComponent(target, previousStateData, t, fitter);
        }

        protected override void UpdateComponent(Component target, SnapshotData previousStateData, float t, AspectRatioFitter component)
        {
            if (t < 1f) return;
            component.aspectMode = aspectMode;
            component.aspectRatio = aspectRatio;
        }
    }
}
