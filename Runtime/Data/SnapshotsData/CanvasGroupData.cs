using System;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.UI.Snapshots;
using Object = UnityEngine.Object; // Added for LayoutAction

namespace AspectSwitcher
{
    [MovedFrom(true, sourceNamespace: "", sourceAssembly: null, sourceClassName: null)]
    [Serializable]
    public class CanvasGroupData : AspectSwitcher.ComponentSnapshotData<CanvasGroup>
    {

        public float alpha         = 1f;
        public bool  interactable  = true;
        public bool  blocksRaycasts = true;

        public override void CaptureFrom(Component target)
        {
            if (target == null) return;
            var cg = target.gameObject.GetComponent<CanvasGroup>();
            if (cg != null)
            {
                action = LayoutAction.Update;
                alpha          = cg.alpha;
                interactable   = cg.interactable;
                blocksRaycasts = cg.blocksRaycasts;
            }
            else
            {
                action = LayoutAction.Remove;
            }
        }

        protected override void CreateComponent(Component target, SnapshotData previousStateData, float t)
        {
            var cg = target.gameObject.AddComponent<CanvasGroup>();
            UpdateComponent(target, previousStateData, t, cg);
        }

        protected override void UpdateComponent(Component target, SnapshotData previousStateData, float t, CanvasGroup cg)
        {
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
