using System;
using System.Collections.Generic;
using UnityEngine;

namespace AspectSwitcher
{
    [Serializable]
    public class ComponentFieldSnapshotEntry : SnapshotEntry<ComponentFieldData>
    {
    }
    [AddComponentMenu("Aspect Switcher/Snapshots/Component Field Snapshot")]
    public sealed class ComponentFieldSnapshot : AspectSnapshot<ComponentFieldData, ComponentFieldSnapshotEntry>
    {
    }
}
