using System;
using System.Collections.Generic;
using UnityEngine;

namespace AspectSwitcher
{
    [Serializable]
    public class CanvasGroupSnapshotEntry : SnapshotEntry<CanvasGroupData>
    {
        [field: SerializeField] public override CanvasGroupData data { get; set; }
    }
    
    [AddComponentMenu("Aspect Switcher/Snapshots/Canvas Group Snapshot")]
    public sealed class CanvasGroupSnapshot : AspectSnapshot<CanvasGroupData, CanvasGroupSnapshotEntry>
    {
        
        protected override Component FindDefaultTarget()                  => GetComponent<CanvasGroup>();
    }
}
