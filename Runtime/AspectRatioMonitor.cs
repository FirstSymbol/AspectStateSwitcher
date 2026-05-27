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

        private struct AspectRatioMonitorUpdate { }

        // Runs before any MonoBehaviour on every domain reload / play-mode entry.
        // Injects a single Tick into the Unity player loop so no MonoBehaviour
        // Update is needed anywhere in the plugin.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void DomainReset()
        {
            CurrentAspect   = 0f;
            OnAspectChanged = null;

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

                // Remove stale entry from a previous domain reload, then re-add.
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

        // Called once per frame by the player loop — the single check point for
        // aspect ratio changes across the entire plugin.
        private static void Tick()
        {
            if (Screen.height == 0) return;
            float aspect = (float)Screen.width / Screen.height;

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

        // Seeds the initial value on Start before the first change event fires.
        public static void Initialize()
        {
            if (Screen.height == 0) return;
            CurrentAspect = (float)Screen.width / Screen.height;
        }

        public static void Reset() => CurrentAspect = 0f;
    }
}
