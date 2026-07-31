using AspectSwitcher;

namespace UnityEngine.UI.Snapshots
{
    public class ComponentEnabledSnapshotData : SnapshotData
    {
        public bool enabled;
        public override void CaptureFrom(Component target)
        {
            enabled = ((MonoBehaviour)target).enabled;
        }

        public override void ApplyTo(Component target, SnapshotData previousStateData, float t)
        {
            if (previousStateData == null || t >= 1f)
            {
                ((MonoBehaviour)target).enabled = enabled;
            }
        }
    }
}