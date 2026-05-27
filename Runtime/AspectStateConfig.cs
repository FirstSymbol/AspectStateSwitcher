using System;
using System.Collections.Generic;
using UnityEngine;

namespace AspectSwitcher
{
    [Serializable]
    public class AspectStateDefinition
    {
        public AspectState state = AspectState.Portrait;
        public AspectRange range = new AspectRange(float.MinValue, float.MaxValue);
    }

    [CreateAssetMenu(menuName = "ARSS/Aspect State Config", fileName = "AspectStateConfig")]
    public class AspectStateConfig : ScriptableObject
    {
        [Tooltip("Aspect ratio ranges for each state. First match wins.")]
        public List<AspectStateDefinition> states = new List<AspectStateDefinition>();

        public AspectState? FindState(float aspect)
        {
            for (int i = 0; i < states.Count; i++)
                if (states[i].range.Matches(aspect))
                    return states[i].state;
            return null;
        }
    }
}
