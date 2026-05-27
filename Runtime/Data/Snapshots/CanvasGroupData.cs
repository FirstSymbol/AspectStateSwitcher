using System;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace AspectSwitcher
{
    [MovedFrom(true, sourceNamespace: "", sourceAssembly: null, sourceClassName: null)]
    [Serializable]
    public class CanvasGroupData : ISnapshotData
    {
        public float alpha         = 1f;
        public bool  interactable  = true;
        public bool  blocksRaycasts = true;

        public void CaptureFrom(Component target)
        {
            if (!(target is CanvasGroup cg)) return;
            alpha          = cg.alpha;
            interactable   = cg.interactable;
            blocksRaycasts = cg.blocksRaycasts;
        }

        public void ApplyTo(Component target, ISnapshotData from, float t)
        {
            if (!(target is CanvasGroup cg)) return;
            var f = from as CanvasGroupData;
            float fromAlpha = f?.alpha ?? cg.alpha;
            cg.alpha = t < 1f ? Mathf.Lerp(fromAlpha, alpha, t) : alpha;
            if (t >= 1f)
            {
                cg.interactable   = interactable;
                cg.blocksRaycasts = blocksRaycasts;
            }
        }
    }
}
