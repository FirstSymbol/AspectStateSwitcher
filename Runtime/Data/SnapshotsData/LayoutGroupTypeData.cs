using System;
using UnityEngine;
using UnityEngine.UI;

namespace AspectSwitcher
{
    public enum LayoutGroupType
    {
        Horizontal,
        Vertical
    }

    [Serializable]
    public class LayoutGroupTypeData : SnapshotData
    {
        public LayoutGroupType layoutType;
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

            var targetObject = target.gameObject;
            var horizontal = target as HorizontalLayoutGroup ?? targetObject.GetComponent<HorizontalLayoutGroup>();
            if (horizontal != null)
            {
                layoutType = LayoutGroupType.Horizontal;
                CaptureHorizontal(horizontal);
                return;
            }

            var vertical = target as VerticalLayoutGroup ?? targetObject.GetComponent<VerticalLayoutGroup>();
            if (vertical == null) return;

            layoutType = LayoutGroupType.Vertical;
            CaptureVertical(vertical);
        }

        public override void ApplyTo(Component target, SnapshotData previousStateData, float t)
        {
            if (layoutType == LayoutGroupType.Horizontal)
            {
                var horizontal = GetOrCreateHorizontal(target);
                if (horizontal != null) ApplyHorizontal(horizontal);
                return;
            }

            var vertical = GetOrCreateVertical(target);
            if (vertical != null) ApplyVertical(vertical);
        }

        private HorizontalLayoutGroup GetOrCreateHorizontal(Component target)
        {
            if (target == null) return null;

            var targetObject = target.gameObject;
            var horizontal = targetObject.GetComponent<HorizontalLayoutGroup>();
            if (horizontal != null) return horizontal;

            var vertical = targetObject.GetComponent<VerticalLayoutGroup>();
            if (vertical != null) UnityEngine.Object.DestroyImmediate(vertical);
            return targetObject.AddComponent<HorizontalLayoutGroup>();
        }

        private VerticalLayoutGroup GetOrCreateVertical(Component target)
        {
            if (target == null) return null;

            var targetObject = target.gameObject;
            var vertical = targetObject.GetComponent<VerticalLayoutGroup>();
            if (vertical != null) return vertical;

            var horizontal = targetObject.GetComponent<HorizontalLayoutGroup>();
            if (horizontal != null) UnityEngine.Object.DestroyImmediate(horizontal);
            return targetObject.AddComponent<VerticalLayoutGroup>();
        }

        private void CaptureHorizontal(HorizontalLayoutGroup layout)
        {
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

        private void CaptureVertical(VerticalLayoutGroup layout)
        {
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

        private void ApplyHorizontal(HorizontalLayoutGroup layout)
        {
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

        private void ApplyVertical(VerticalLayoutGroup layout)
        {
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
