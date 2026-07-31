using System;
using System.Collections.Generic;
using AspectSwitcher;
using UnityEngine.Scripting.APIUpdating;

namespace UnityEngine.UI.Snapshots
{
    [Serializable]
    [MovedFrom(true, sourceNamespace: "AspectSwitcher", sourceAssembly: "AspectRatioSwitcher.Runtime", sourceClassName: "GOSetActiveSnapshotEntry")]
    public class GameObjectActiveSnapshotEntry : SnapshotEntry<GameObjectActiveData>
    {
    }
    [MovedFrom(true, sourceNamespace: "AspectSwitcher", sourceAssembly: "AspectRatioSwitcher.Runtime", sourceClassName: "GOSetActiveSnapshot")]
    public class GameObjectActiveSnapshot : AspectSnapshot<GameObjectActiveData, GameObjectActiveSnapshotEntry>
    {
        protected override Component FindDefaultTarget()                  => transform;
    }
}