using System;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace AspectSwitcher
{
    [MovedFrom(true, sourceNamespace: "", sourceAssembly: null, sourceClassName: null)]
    [Serializable]
    public class TransformData : SnapshotData
    {
        public Vector3    localPosition;
        public Quaternion localRotation;
        public Vector3    localScale;

        public override void CaptureFrom(Component target)
        {
            if (!(target is Transform tr)) return;
            localPosition = tr.localPosition;
            localRotation = tr.localRotation;
            localScale    = tr.localScale;
        }

        public override void ApplyTo(Component target, SnapshotData previousStateData, float t)
        {
            if (!(target is Transform tr)) return;
            var f = previousStateData as TransformData;
            if (f != null && t < 1f)
            {
                tr.localPosition = Vector3.Lerp(f.localPosition, localPosition, t);
                tr.localRotation = Quaternion.Lerp(f.localRotation, localRotation, t);
                tr.localScale    = Vector3.Lerp(f.localScale, localScale, t);
            }
            else
            {
                tr.localPosition = localPosition;
                tr.localRotation = localRotation;
                tr.localScale    = localScale;
            }
        }
    }
}
