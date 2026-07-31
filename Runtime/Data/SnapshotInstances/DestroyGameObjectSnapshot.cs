using System;
using AspectSwitcher;
using UnityEngine;

namespace UnityEngine.UI.Snapshots
{
    [Serializable]
    public class DestroyComponentSnapshotEntry : SnapshotEntry<DestroyGameObjectSnapshotData> { }

    public class DestroyGameObjectSnapshot : AspectSnapshot<DestroyGameObjectSnapshotData, DestroyComponentSnapshotEntry>
    {
    }
}
