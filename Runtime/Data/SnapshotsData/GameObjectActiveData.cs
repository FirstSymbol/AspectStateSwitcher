using System;
using AspectSwitcher;
using UnityEngine.Scripting.APIUpdating;

namespace UnityEngine.UI.Snapshots
{
    [Serializable]
    [MovedFrom(true, sourceNamespace: null, sourceAssembly: null, sourceClassName: "GOSetActiveData")]
    public class GameObjectActiveData : SnapshotData
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