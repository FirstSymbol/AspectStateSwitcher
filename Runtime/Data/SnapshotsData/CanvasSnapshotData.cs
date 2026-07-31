using System;
using AspectSwitcher;
using UnityEngine;

namespace UnityEngine.UI.Snapshots
{
    [Serializable]
    public class CanvasSnapshotData : SnapshotData
    {
        public LayoutAction action = LayoutAction.Update;

        public RenderMode renderMode = RenderMode.ScreenSpaceOverlay;
        public bool pixelPerfect = false;
        public int sortingOrder = 0;
        public int sortingLayerID = 0;
        public bool overrideSorting = false;
        public bool overridePixelPerfect = false;
        public float planeDistance = 100f;

        public override void CaptureFrom(Component target)
        {
            if (target == null) return;

            var component = target.gameObject.GetComponent<Canvas>();
            if (component != null)
            {
                action = LayoutAction.Update;
                renderMode = component.renderMode;
                pixelPerfect = component.pixelPerfect;
                sortingOrder = component.sortingOrder;
                sortingLayerID = component.sortingLayerID;
                overrideSorting = component.overrideSorting;
                overridePixelPerfect = component.overridePixelPerfect;
                planeDistance = component.planeDistance;
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
                var existing = go.GetComponent<Canvas>();
                if (existing != null)
                    Object.DestroyImmediate(existing);
                return;
            }

            var component = go.GetComponent<Canvas>();

            if (component == null && action != LayoutAction.Update)
                component = go.AddComponent<Canvas>();

            if (component == null) return;

            component.renderMode = renderMode;
            component.pixelPerfect = pixelPerfect;
            component.sortingOrder = sortingOrder;
            component.sortingLayerID = sortingLayerID;
            component.overrideSorting = overrideSorting;
            component.overridePixelPerfect = overridePixelPerfect;
            component.planeDistance = planeDistance;
        }
    }
}
