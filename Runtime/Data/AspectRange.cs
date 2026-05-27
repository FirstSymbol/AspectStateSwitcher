using System;
using UnityEngine;

namespace AspectSwitcher
{
    [Serializable]
    public struct AspectRange
    {
        public float min;
        public float max;

        public AspectRange(float min, float max)
        {
            this.min = min;
            this.max = max;
        }

        public bool Matches(float aspect) => aspect >= min && aspect < max;

        public static AspectRange FromMin(float min) => new AspectRange(min, float.MaxValue);
        public static AspectRange ToMax(float max)   => new AspectRange(float.MinValue, max);
        public static AspectRange Full               => new AspectRange(float.MinValue, float.MaxValue);
    }
}
