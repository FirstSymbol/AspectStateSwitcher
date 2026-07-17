using System.Collections.Generic;

namespace AspectSwitcher
{
    public abstract class AspectSnapshot<TData, TEntry> : AspectSnapshotBase where TData : SnapshotData, new() where TEntry : SnapshotEntry<TData>, new()
    {
        public new List<TEntry> entries = new List<TEntry>();
        
        public new virtual TData CreateSnapshotData() => new();

        protected new virtual TData FindDataForState(AspectState state)
        {
            for (int i = 0; i < entries.Count; i++)
                if (entries[i].states.Contains(state)) return entries[i].data;
            return null;
        }
        public override SnapshotData GetDataAt(int index)
        {
            if (index < 0) return null;
    
            while (entries.Count <= index)
            {
                entries.Add(new TEntry());
            }
    
            if (entries[index] == null)
            {
                entries[index] = new TEntry();
            }
    
            return entries[index].data;
        }
        
    }
}
