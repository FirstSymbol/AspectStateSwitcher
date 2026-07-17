using System;
using System.Collections.Generic;
using UnityEngine;

namespace AspectSwitcher
{
    [Serializable]
    public abstract class SnapshotEntry<TData> : ISerializationCallbackReceiver where TData : SnapshotData
    {
        [HideInInspector] public AspectState state;
        public List<AspectState> states = new List<AspectState>();
        [field: SerializeField] public abstract TData data {get; set; }

        public virtual void OnBeforeSerialize() { }
        public virtual void OnAfterDeserialize()
        {
            if (states.Count == 0)
                states.Add(state);
        }
    }
}