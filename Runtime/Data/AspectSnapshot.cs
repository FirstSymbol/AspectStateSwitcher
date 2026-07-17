using System.Collections.Generic;

namespace AspectSwitcher
{
    public abstract class AspectSnapshot<TData, TEntry> : AspectSnapshotBase where TData : SnapshotData, new() where TEntry : SnapshotEntry<TData>, new()
    {
        public List<TEntry> entries = new List<TEntry>(); // Убрали 'new', теперь этот список единственный и верный
        
        public override SnapshotData CreateSnapshotData() => new();

        // Теперь этот метод честно переопределяет базовый и вызывается из AspectSnapshotBase
        protected override SnapshotData FindDataForState(AspectState state)
        {
            for (int i = 0; i < entries.Count; i++)
            {
                if (entries[i] != null && entries[i].states != null && entries[i].states.Contains(state)) 
                    return entries[i].data;
            }
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