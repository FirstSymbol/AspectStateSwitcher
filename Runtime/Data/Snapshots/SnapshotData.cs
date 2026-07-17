using UnityEngine;

namespace AspectSwitcher
{
    public class SnapshotData
    {
        public virtual void CaptureFrom(Component target){}
        public virtual void ApplyTo(Component target, SnapshotData previousStateData, float t){}
    }
}
