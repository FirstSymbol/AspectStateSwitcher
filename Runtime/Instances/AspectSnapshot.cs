using System.Collections.Generic;

namespace AspectSwitcher
{
    public abstract class AspectSnapshot<TData, TEntry> : AspectSnapshotBase where TData : SnapshotData, new() where TEntry : SnapshotEntry<TData>
    {
        public new List<TEntry> entries = new List<TEntry>();
        
        public new virtual TData CreateSnapshotData() => new();

        protected new virtual TData FindDataForState(AspectState state)
        {
            for (int i = 0; i < entries.Count; i++)
                if (entries[i].states.Contains(state)) return entries[i].data;
            return null;
        }
        public new virtual TData GetDataAt(int index) => entries[index].data;
        
    }
}
