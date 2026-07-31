using System;
using AspectSwitcher;
using UnityEngine;
using UnityEngine.UI;

namespace UnityEngine.UI.Snapshots
{
    [Serializable]
    public class HorizontalLayoutSnapshotData : AspectSwitcher.ComponentSnapshotData<HorizontalLayoutGroup>
    {

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
                action = LayoutAction.Update;
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

        protected override void CreateComponent(Component target, SnapshotData previousStateData, float t)
        {
            if (t < 1f) return;
            var layout = target.gameObject.AddComponent<HorizontalLayoutGroup>();
            UpdateComponent(target, previousStateData, t, layout);
        }

        protected override void UpdateComponent(Component target, SnapshotData previousStateData, float t, HorizontalLayoutGroup component)
        {
            if (t < 1f) return;
            component.padding = CopyPadding(padding);
            component.spacing = spacing;
            component.childAlignment = childAlignment;
            component.reverseArrangement = reverseArrangement;
            component.childForceExpandWidth = childForceExpandWidth;
            component.childForceExpandHeight = childForceExpandHeight;
            component.childControlWidth = childControlWidth;
            component.childControlHeight = childControlHeight;
            component.childScaleWidth = childScaleWidth;
            component.childScaleHeight = childScaleHeight;
        }

        private static RectOffset CopyPadding(RectOffset source)
        {
            return source == null
                ? new RectOffset()
                : new RectOffset(source.left, source.right, source.top, source.bottom);
        }
    }
}
