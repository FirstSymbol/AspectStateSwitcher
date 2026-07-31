using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Snapshots;
using Object = UnityEngine.Object; // Added for LayoutAction

namespace AspectSwitcher
{
    [Serializable]
    public class HorizontalOrVerticalLayoutGroupData : AspectSwitcher.ComponentSnapshotData<HorizontalOrVerticalLayoutGroup>
    {
        public bool isHorizontal = true; // Added to distinguish layout types when creating

        public RectOffset padding = new RectOffset();
        public float spacing = 0;
        public TextAnchor childAlignment = TextAnchor.UpperLeft;
        public bool reverseArrangement;
        public bool childForceExpandWidth = true;
        public bool childForceExpandHeight = true;
        public bool childControlWidth = true;
        public bool childControlHeight = true;
        public bool childScaleWidth = false;
        public bool childScaleHeight = false;

        public override void CaptureFrom(Component target)
        {
            if (target == null) return;
            
            var layoutGroup = target.gameObject.GetComponent<HorizontalOrVerticalLayoutGroup>();
            if (layoutGroup != null)
            {
                action = LayoutAction.Update;
                isHorizontal = layoutGroup is HorizontalLayoutGroup;
                padding = CopyPadding(layoutGroup.padding);
                spacing = layoutGroup.spacing;
                childAlignment = layoutGroup.childAlignment;
                reverseArrangement = layoutGroup.reverseArrangement;
                childForceExpandWidth = layoutGroup.childForceExpandWidth;
                childForceExpandHeight = layoutGroup.childForceExpandHeight;
                childControlWidth = layoutGroup.childControlWidth;
                childControlHeight = layoutGroup.childControlHeight;
                childScaleWidth = layoutGroup.childScaleWidth;
                childScaleHeight = layoutGroup.childScaleHeight;
            }
            else
            {
                action = LayoutAction.Remove;
            }
        }

        protected override void CreateComponent(Component target, SnapshotData previousStateData, float t)
        {
            HorizontalOrVerticalLayoutGroup layoutGroup;
            if (isHorizontal)
                layoutGroup = target.gameObject.AddComponent<HorizontalLayoutGroup>();
            else
                layoutGroup = target.gameObject.AddComponent<VerticalLayoutGroup>();
                
            UpdateComponent(target, previousStateData, t, layoutGroup);
        }

        protected override void UpdateComponent(Component target, SnapshotData previousStateData, float t, HorizontalOrVerticalLayoutGroup layoutGroup)
        {
            var previousLayoutData = previousStateData as HorizontalOrVerticalLayoutGroupData;
            if (previousLayoutData == null || t >= 1f)
            {
                layoutGroup.padding = CopyPadding(padding);
                layoutGroup.spacing = spacing;
                layoutGroup.childAlignment = childAlignment;
                layoutGroup.reverseArrangement = reverseArrangement;
                layoutGroup.childForceExpandWidth = childForceExpandWidth;
                layoutGroup.childForceExpandHeight = childForceExpandHeight;
                layoutGroup.childControlWidth = childControlWidth;
                layoutGroup.childControlHeight = childControlHeight;
                layoutGroup.childScaleWidth = childScaleWidth;
                layoutGroup.childScaleHeight = childScaleHeight;
                return;
            }
            
            layoutGroup.spacing = Mathf.Lerp(previousLayoutData.spacing, spacing, t);
            layoutGroup.padding.bottom = (int)Mathf.Lerp(previousLayoutData.padding.bottom, padding.bottom, t);
            layoutGroup.padding.left = (int)Mathf.Lerp(previousLayoutData.padding.left, padding.left, t);
            layoutGroup.padding.right = (int)Mathf.Lerp(previousLayoutData.padding.right, padding.right, t);
            layoutGroup.padding.top = (int)Mathf.Lerp(previousLayoutData.padding.top, padding.top, t);
        }

        private static RectOffset CopyPadding(RectOffset source)
        {
            return source == null
                ? new RectOffset()
                : new RectOffset(source.left, source.right, source.top, source.bottom);
        }
    }
}