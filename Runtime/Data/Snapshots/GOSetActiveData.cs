using System;
using UnityEngine;

namespace AspectSwitcher
{
    [Serializable]
    public class GOSetActiveData : SnapshotData
    {
        public bool active;
        public override void CaptureFrom(Component target)
        {
            active = target.gameObject.activeSelf;
        }

        public override void ApplyTo(Component target, SnapshotData previousStateData, float t)
        {
            target.gameObject.SetActive(active);
        }
    }
}