using System;
using AspectSwitcher;
using UnityEngine;
using UnityEngine.UI;

namespace UnityEngine.UI.Snapshots
{
    [Serializable]
    public class GridLayoutSnapshotEntry : SnapshotEntry<GridLayoutSnapshotData> { }

    public class GridLayoutSnapshot : AspectSnapshot<GridLayoutSnapshotData, GridLayoutSnapshotEntry>
    {
        protected override Component FindDefaultTarget() => transform;

        private void OnValidate()
        {
            if (target is GridLayoutGroup grid)
                target = grid.transform;
        }
    }
}
