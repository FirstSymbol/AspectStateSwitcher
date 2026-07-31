using System;
using UnityEngine;
using UnityEngine.UI.Snapshots;

namespace AspectSwitcher
{
    [Serializable]
    public abstract class ComponentSnapshotData<T> : SnapshotData where T : Component
    {
        public LayoutAction action = LayoutAction.Update;
        public bool updateIfAlreadyCreated = true;
        public bool createIfComponentNotFound = true;

        public sealed override void ApplyTo(Component target, SnapshotData previousStateData, float t)
        {
            if (target == null) return;
            
            var go = target.gameObject;
            var component = go.GetComponent<T>();
            bool hasComponent = component != null;

            if (action == LayoutAction.Update)
            {
                if (hasComponent) 
                    UpdateComponent(target, previousStateData, t, component);
                else if (createIfComponentNotFound) 
                    CreateComponent(target, previousStateData, t);
            }
            else if (action == LayoutAction.Create)
            {
                if (!hasComponent) 
                    CreateComponent(target, previousStateData, t);
                else if (updateIfAlreadyCreated) 
                    UpdateComponent(target, previousStateData, t, component);
                else 
                    CreateComponent(target, previousStateData, t);
            }
            else if (action == LayoutAction.Remove)
            {
                if (hasComponent) 
                    RemoveComponent(target, previousStateData, t, component);
            }
        }

        protected abstract void CreateComponent(Component target, SnapshotData previousStateData, float t);
        protected abstract void UpdateComponent(Component target, SnapshotData previousStateData, float t, T component);
        
        protected virtual void RemoveComponent(Component target, SnapshotData previousStateData, float t, T component)
        {
            if (t >= 1f && component != null)
            {
                UnityEngine.Object.DestroyImmediate(component);
            }
        }
    }
}
