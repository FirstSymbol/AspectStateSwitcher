using System;
using AspectSwitcher;
using UnityEngine;
using UnityEngine.UI;

namespace UnityEngine.UI.Snapshots
{
    [Serializable]
    public class CanvasScalerSnapshotData : AspectSwitcher.ComponentSnapshotData<CanvasScaler>
    {

        public CanvasScaler.ScaleMode uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
        public float scaleFactor = 1f;
        public float referencePixelsPerUnit = 100f;
        public Vector2 referenceResolution = new Vector2(800f, 600f);
        public CanvasScaler.ScreenMatchMode screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        [Range(0, 1)] public float matchWidthOrHeight = 0f;
        public CanvasScaler.Unit physicalUnit = CanvasScaler.Unit.Points;
        public float fallbackScreenDPI = 96f;
        public float defaultSpriteDPI = 96f;
        public float dynamicPixelsPerUnit = 1f;

        public override void CaptureFrom(Component target)
        {
            if (target == null) return;

            var component = target.gameObject.GetComponent<CanvasScaler>();
            if (component != null)
            {
                action = LayoutAction.Update;
                uiScaleMode = component.uiScaleMode;
                scaleFactor = component.scaleFactor;
                referencePixelsPerUnit = component.referencePixelsPerUnit;
                referenceResolution = component.referenceResolution;
                screenMatchMode = component.screenMatchMode;
                matchWidthOrHeight = component.matchWidthOrHeight;
                physicalUnit = component.physicalUnit;
                fallbackScreenDPI = component.fallbackScreenDPI;
                defaultSpriteDPI = component.defaultSpriteDPI;
                dynamicPixelsPerUnit = component.dynamicPixelsPerUnit;
            }
            else
            {
                action = LayoutAction.Remove;
            }
        }

        protected override void CreateComponent(Component target, SnapshotData previousStateData, float t)
        {
            if (t < 1f) return;
            var component = target.gameObject.AddComponent<CanvasScaler>();
            UpdateComponent(target, previousStateData, t, component);
        }

        protected override void UpdateComponent(Component target, SnapshotData previousStateData, float t, CanvasScaler component)
        {
            if (t < 1f) return;
            component.uiScaleMode = uiScaleMode;
            component.scaleFactor = scaleFactor;
            component.referencePixelsPerUnit = referencePixelsPerUnit;
            component.referenceResolution = referenceResolution;
            component.screenMatchMode = screenMatchMode;
            component.matchWidthOrHeight = matchWidthOrHeight;
            component.physicalUnit = physicalUnit;
            component.fallbackScreenDPI = fallbackScreenDPI;
            component.defaultSpriteDPI = defaultSpriteDPI;
            component.dynamicPixelsPerUnit = dynamicPixelsPerUnit;
        }
    }
}
