using System;
using System.Collections.Generic;
using UnityEngine;

namespace AspectSwitcher
{
    [Serializable]
    public class AnimatorSnapshotEntry : SnapshotEntry<AnimatorParamData>
    {
        public override AnimatorParamData data { get; set; }
    }
    [AddComponentMenu("Aspect Switcher/Snapshots/Animator Param Snapshot")]
    public sealed class AnimatorParamSnapshot : AspectSnapshot<AnimatorParamData, AnimatorSnapshotEntry>
    {
        
        
        protected override Component FindDefaultTarget()                  => GetComponent<Animator>();
    }
}
