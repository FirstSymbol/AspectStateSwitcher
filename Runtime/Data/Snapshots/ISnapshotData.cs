using UnityEngine;

namespace AspectSwitcher
{
    public interface ISnapshotData
    {
        void CaptureFrom(Component target);
        void ApplyTo(Component target, ISnapshotData from, float t);
    }
}
