using System;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace AspectSwitcher
{
    [MovedFrom(true, sourceNamespace: "", sourceAssembly: null, sourceClassName: null)]
    [Serializable]
    public class UITransformData : SnapshotData
    {
        public Vector2 anchorMin;
        public Vector2 anchorMax;
        public Vector2 anchoredPosition;
        public Vector2 sizeDelta;
        public Vector2 pivot;
        public Vector3 localScale = Vector3.one;

        public override void CaptureFrom(Component target)
        {
            if (!(target is RectTransform rt)) return;
            anchorMin        = rt.anchorMin;
            anchorMax        = rt.anchorMax;
            anchoredPosition = rt.anchoredPosition;
            sizeDelta        = rt.sizeDelta;
            pivot            = rt.pivot;
            localScale = rt.localScale;
        }

        public override void ApplyTo(Component target, SnapshotData previousStateData, float t)
        {
            if (!(target is RectTransform rt)) return;
            var f = previousStateData as UITransformData;

            if (f == null || t >= 1f)
            {
                rt.anchorMin        = anchorMin;
                rt.anchorMax        = anchorMax;
                rt.pivot            = pivot;
                rt.anchoredPosition = anchoredPosition;
                rt.sizeDelta        = sizeDelta;
                rt.localScale       = localScale;
                return;
            }

            // During transition: do NOT change anchors/pivot — Unity would
            // immediately recalculate anchoredPosition to compensate, causing
            // the element to visually snap to the target before lerp finishes.
            rt.anchoredPosition = Vector2.Lerp(f.anchoredPosition, anchoredPosition, t);
            rt.sizeDelta        = Vector2.Lerp(f.sizeDelta,        sizeDelta,        t);
            rt.localScale       = Vector3.Lerp(f.localScale,       localScale,       t);
        }
    }
}
