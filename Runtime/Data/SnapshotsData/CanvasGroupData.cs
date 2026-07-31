using System;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace AspectSwitcher
{
    [MovedFrom(true, sourceNamespace: "", sourceAssembly: null, sourceClassName: null)]
    [Serializable]
    public class CanvasGroupData : SnapshotData
    {
        public float alpha         = 1f;
        public bool  interactable  = true;
        public bool  blocksRaycasts = true;

        public override void CaptureFrom(Component target)
        {
            if (!(target is CanvasGroup cg)) return;
            alpha          = cg.alpha;
            interactable   = cg.interactable;
            blocksRaycasts = cg.blocksRaycasts;
        }

        public override void ApplyTo(Component target, SnapshotData previousStateData, float t)
        {
            if (!(target is CanvasGroup cg)) return;
            var f = previousStateData as CanvasGroupData;
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
