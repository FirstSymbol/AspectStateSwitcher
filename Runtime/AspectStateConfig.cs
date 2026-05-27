using System;
using System.Collections.Generic;
using UnityEngine;

namespace AspectSwitcher
{
    [Serializable]
    public class AspectStateDefinition
    {
        public AspectState state        = AspectState.Portrait;
        public AspectRange range        = new AspectRange(float.MinValue, float.MaxValue);
        public bool        showInDiagram = true;
    }

    [CreateAssetMenu(menuName = "ARSS/Aspect State Config", fileName = "AspectStateConfig")]
    public class AspectStateConfig : ScriptableObject
    {
        [Tooltip("Aspect ratio ranges for each state. First match wins.")]
        public List<AspectStateDefinition> states = new List<AspectStateDefinition>();

        private void Reset()
        {
            states = new List<AspectStateDefinition>
            {
                new AspectStateDefinition { state = AspectState.Portrait,         range = new AspectRange(float.MinValue, 0.75f),      showInDiagram = true },
                new AspectStateDefinition { state = AspectState.Tall,             range = new AspectRange(0.75f,         1f),          showInDiagram = true },
                new AspectStateDefinition { state = AspectState.Compact,          range = new AspectRange(1f,            1.333333f),   showInDiagram = true },
                new AspectStateDefinition { state = AspectState.Tablet,           range = new AspectRange(0.75f,         1.333333f),   showInDiagram = true },
                new AspectStateDefinition { state = AspectState.PortraitTall,     range = new AspectRange(float.MinValue, 1f),         showInDiagram = true },
                new AspectStateDefinition { state = AspectState.CompactLandscape, range = new AspectRange(1f,            1.78f),       showInDiagram = true },
                new AspectStateDefinition { state = AspectState.Landscape,        range = new AspectRange(1.333333f,     1.78f),       showInDiagram = true },
                new AspectStateDefinition { state = AspectState.Wide,             range = new AspectRange(1.78f,         float.MaxValue), showInDiagram = true },
            };
        }

        public AspectState? FindState(float aspect)
        {
            for (int i = 0; i < states.Count; i++)
                if (states[i].range.Matches(aspect))
                    return states[i].state;
            return null;
        }

        public void GetMatchingStates(float aspect, List<AspectState> results)
        {
            results.Clear();
            for (int i = 0; i < states.Count; i++)
                if (states[i].range.Matches(aspect))
                    results.Add(states[i].state);
        }

        public void GetContainedStates(AspectState state, List<AspectState> results)
        {
            results.Clear();
            AspectRange clickedRange = default;
            bool found = false;
            for (int i = 0; i < states.Count; i++)
            {
                if (states[i].state == state) { clickedRange = states[i].range; found = true; break; }
            }
            if (!found) { results.Add(state); return; }

            for (int i = 0; i < states.Count; i++)
                if (states[i].range.IsContainedIn(clickedRange))
                    results.Add(states[i].state);
        }
    }
}
