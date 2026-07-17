using System;
using System.Collections.Generic;
using UnityEngine;

namespace AspectSwitcher
{
    [Serializable]
    public abstract class SnapshotEntryBase : ISerializationCallbackReceiver
    {
        [HideInInspector] public AspectState state;
        public List<AspectState> states = new List<AspectState>();
        // Оставляем абстрактный доступ к данным для рантайм-кода
        public abstract SnapshotData BaseData { get; }

        public virtual void OnBeforeSerialize() { }
        public virtual void OnAfterDeserialize()
        {
            if (states.Count == 0)
                states.Add(state);
        }
    }

    [Serializable]
    public abstract class SnapshotEntry<TData> : SnapshotEntryBase where TData : SnapshotData
    {
        public TData _data;
        public TData data { get => _data; set => _data = value; }

        // Реализуем базовый доступ
        public override SnapshotData BaseData => data;
    }
}