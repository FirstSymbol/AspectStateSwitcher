using System;
using AspectSwitcher;
using UnityEngine;

namespace UnityEngine.UI.Snapshots
{
    [Serializable]
    public class CanvasSnapshotData : AspectSwitcher.ComponentSnapshotData<Canvas>
    {

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

        protected override void CreateComponent(Component target, SnapshotData previousStateData, float t)
        {
            if (t < 1f) return;
            var component = target.gameObject.AddComponent<Canvas>();
            UpdateComponent(target, previousStateData, t, component);
        }

        protected override void UpdateComponent(Component target, SnapshotData previousStateData, float t, Canvas component)
        {
            if (t < 1f) return;
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
