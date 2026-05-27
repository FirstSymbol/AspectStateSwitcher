using System;
using UnityEngine;

namespace AspectSwitcher
{
    public static class AspectRatioMonitor
    {
        public static float CurrentAspect { get; private set; }
        public static event Action<float> OnAspectChanged;
        public static float Threshold = 0.01f;

        private static bool _initialized;

        public static void Tick()
        {
            if (Screen.height == 0) return;
            float aspect = (float)Screen.width / Screen.height;
            if (!_initialized)
            {
                _initialized = true;
                CurrentAspect = aspect;
                return;
            }
            if (Mathf.Abs(aspect - CurrentAspect) > Threshold)
            {
                CurrentAspect = aspect;
                OnAspectChanged?.Invoke(CurrentAspect);
            }
        }

        public static void Reset()
        {
            _initialized = false;
            CurrentAspect = 0f;
        }
    }
}
