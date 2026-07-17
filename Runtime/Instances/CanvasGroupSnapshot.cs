using System;
using System.Collections.Generic;
using UnityEngine;

namespace AspectSwitcher
{
    [Serializable]
    public class CanvasGroupSnapshotEntry : SnapshotEntry<CanvasGroupData>
    {
    }
    
    [AddComponentMenu("Aspect Switcher/Snapshots/Canvas Group Snapshot")]
    public sealed class CanvasGroupSnapshot : AspectSnapshot<CanvasGroupData, CanvasGroupSnapshotEntry>
    {
        protected override Component FindDefaultTarget()                  => GetComponent<CanvasGroup>();
    }
}
