using System;
using UnityEngine;
using UnityEngine.UI;

namespace AspectSwitcher
{
    [Serializable]
    public class HorizontalOrVerticalLayoutGroupData : SnapshotData
    {
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
            if (!(target is HorizontalOrVerticalLayoutGroup layoutGroup)) return;
            padding =  layoutGroup.padding;
            spacing = layoutGroup.spacing;
            childAlignment = layoutGroup.childAlignment;
            childForceExpandWidth = layoutGroup.childForceExpandWidth;
            childForceExpandHeight = layoutGroup.childForceExpandHeight;
            childControlWidth = layoutGroup.childControlWidth;
            childControlHeight = layoutGroup.childControlHeight;
            childScaleWidth = layoutGroup.childScaleWidth;
            childScaleHeight = layoutGroup.childScaleHeight;
        }

        public override void ApplyTo(Component target, SnapshotData previousStateData, float t)
        {
            if (!(target is HorizontalOrVerticalLayoutGroup layoutGroup)) return;
            var previousLayoutData = previousStateData as HorizontalOrVerticalLayoutGroupData;
            if (previousLayoutData == null || t >= 1f)
            {
                layoutGroup.padding =  padding;
                layoutGroup.spacing = spacing;
                layoutGroup.childAlignment = childAlignment;
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
    }
}