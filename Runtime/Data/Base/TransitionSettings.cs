using System;
using UnityEngine;
using UnityEngine.Events;

namespace AspectSwitcher
{
    public enum TransitionMode { Instant, Lerp, AnimationCurve }

    [Serializable]
    public class TransitionSettings
    {
        public TransitionMode mode = TransitionMode.Instant;
        public float duration = 0.3f;
        public AnimationCurve curve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        public UnityEvent onComplete;
    }
}
