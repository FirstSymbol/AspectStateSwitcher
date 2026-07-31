using System;
using AspectSwitcher;
using UnityEngine;
using UnityEngine.UI;

namespace UnityEngine.UI.Snapshots
{
    [Serializable]
    public class CanvasScalerSnapshotData : SnapshotData
    {
        public LayoutAction action = LayoutAction.Update;

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

        public override void ApplyTo(Component target, SnapshotData previousStateData, float t)
        {
            if (target == null) return;
            if (t < 1f) return;

            var go = target.gameObject;

            if (action == LayoutAction.Remove)
            {
                var existing = go.GetComponent<CanvasScaler>();
                if (existing != null)
                    Object.DestroyImmediate(existing);
                return;
            }

            var component = go.GetComponent<CanvasScaler>();

            if (component == null && action != LayoutAction.Update)
                component = go.AddComponent<CanvasScaler>();

            if (component == null) return;

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
