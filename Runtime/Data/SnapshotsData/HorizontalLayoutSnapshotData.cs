using System;
using AspectSwitcher;
using UnityEngine;
using UnityEngine.UI;

namespace UnityEngine.UI.Snapshots
{
    [Serializable]
    public class HorizontalLayoutSnapshotData : SnapshotData
    {
        public LayoutAction action = LayoutAction.Update;

        [Header("Layout Settings")]
        public RectOffset padding = new RectOffset();
        public float spacing;
        public TextAnchor childAlignment = TextAnchor.UpperLeft;
        public bool reverseArrangement;
        public bool childForceExpandWidth = true;
        public bool childForceExpandHeight = true;
        public bool childControlWidth = true;
        public bool childControlHeight = true;
        public bool childScaleWidth;
        public bool childScaleHeight;

        public override void CaptureFrom(Component target)
        {
            if (target == null) return;

            var layout = target.gameObject.GetComponent<HorizontalLayoutGroup>();
            if (layout != null)
            {
                action = LayoutAction.Create;
                padding = CopyPadding(layout.padding);
                spacing = layout.spacing;
                childAlignment = layout.childAlignment;
                reverseArrangement = layout.reverseArrangement;
                childForceExpandWidth = layout.childForceExpandWidth;
                childForceExpandHeight = layout.childForceExpandHeight;
                childControlWidth = layout.childControlWidth;
                childControlHeight = layout.childControlHeight;
                childScaleWidth = layout.childScaleWidth;
                childScaleHeight = layout.childScaleHeight;
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
                var existing = go.GetComponent<HorizontalLayoutGroup>();
                if (existing != null)
                    Object.DestroyImmediate(existing);
                return;
            }

            var layout = go.GetComponent<HorizontalLayoutGroup>();

            if (layout == null && action != LayoutAction.Update)
                layout = go.AddComponent<HorizontalLayoutGroup>();

            if (layout == null) return;

            layout.padding = CopyPadding(padding);
            layout.spacing = spacing;
            layout.childAlignment = childAlignment;
            layout.reverseArrangement = reverseArrangement;
            layout.childForceExpandWidth = childForceExpandWidth;
            layout.childForceExpandHeight = childForceExpandHeight;
            layout.childControlWidth = childControlWidth;
            layout.childControlHeight = childControlHeight;
            layout.childScaleWidth = childScaleWidth;
            layout.childScaleHeight = childScaleHeight;
        }

        private static RectOffset CopyPadding(RectOffset source)
        {
            return source == null
                ? new RectOffset()
                : new RectOffset(source.left, source.right, source.top, source.bottom);
        }
    }
}
