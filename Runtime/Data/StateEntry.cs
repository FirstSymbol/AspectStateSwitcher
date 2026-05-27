using System;
using UnityEngine;

namespace AspectSwitcher
{
    [Serializable]
    public class StateEntry
    {
        public AspectState state = AspectState.Portrait;
        [SerializeReference]
        public ISnapshotData data;
    }
}
