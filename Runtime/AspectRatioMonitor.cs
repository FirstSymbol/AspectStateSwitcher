using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;

namespace AspectSwitcher
{
    public static class AspectRatioMonitor
    {
        public static float CurrentAspect { get; private set; }
        public static event Action<float> OnAspectChanged;
        public static float Threshold = 0.01f;
        public static Camera Camera = null;

        private struct AspectRatioMonitorUpdate { }

        // SubsystemRegistration runs before any MonoBehaviour and before Play Mode entry,
        // letting us inject into the player loop without a MonoBehaviour of our own.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void DomainReset()
        {
            CurrentAspect = 0f;
            OnAspectChanged = null;
            Camera = null;

            var loop = PlayerLoop.GetCurrentPlayerLoop();
            InjectIntoLoop(ref loop);
            PlayerLoop.SetPlayerLoop(loop);
        }

        private static void InjectIntoLoop(ref PlayerLoopSystem root)
        {
            for (int i = 0; i < root.subSystemList.Length; i++)
            {
                if (root.subSystemList[i].type != typeof(Update)) continue;

                ref var update = ref root.subSystemList[i];
                var existing   = update.subSystemList ?? Array.Empty<PlayerLoopSystem>();
                
                var rebuilt = new List<PlayerLoopSystem>(existing.Length + 1);
                foreach (var s in existing)
                    if (s.type != typeof(AspectRatioMonitorUpdate))
                        rebuilt.Add(s);

                rebuilt.Add(new PlayerLoopSystem
                {
                    type           = typeof(AspectRatioMonitorUpdate),
                    updateDelegate = Tick
                });

                update.subSystemList = rebuilt.ToArray();
                return;
            }
        }

        private static void Tick()
        {
            if (Camera == null)
            {
                Camera = Camera.main;
                if (Camera == null) return;
            }
            if (Camera.pixelHeight == 0) return;
            float aspect = (float)Camera.pixelWidth / Camera.pixelHeight;

            if (CurrentAspect == 0f)
            {
                CurrentAspect = aspect;
                return;
            }

            if (Mathf.Abs(aspect - CurrentAspect) > Threshold)
            {
                CurrentAspect = aspect;
                OnAspectChanged?.Invoke(CurrentAspect);
            }
        }

        public static void Initialize()
        {
            if (Camera == null)
            {
                Camera = Camera.main;
                if (Camera == null) return;
            }
            if (Camera.pixelHeight == 0) return;
            CurrentAspect = (float)Camera.pixelWidth / Camera.pixelHeight;
        }

        public static void Reset() => CurrentAspect = 0f;
    }
}
