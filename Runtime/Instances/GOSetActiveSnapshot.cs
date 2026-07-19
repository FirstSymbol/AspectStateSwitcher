using System;
using UnityEngine;

namespace AspectSwitcher
{
    [Serializable]
    public class GOSetActiveSnapshotEntry : SnapshotEntry<GOSetActiveData> { }
    public class GOSetActiveSnapshot : AspectSnapshot<GOSetActiveData, GOSetActiveSnapshotEntry>
    {
        protected override Component FindDefaultTarget() => transform;
    }
}