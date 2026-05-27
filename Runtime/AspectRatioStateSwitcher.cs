using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace AspectSwitcher
{
    [Serializable]
    public class AspectStateEvent : UnityEvent<AspectState> {}

    public class AspectRatioStateSwitcher : MonoBehaviour
    {
        public static AspectRatioStateSwitcher Instance { get; private set; }

        public static event Action<AspectState> OnStateChanged;
        public static AspectState? CurrentState { get; private set; }

        [Header("Configuration")]
        public AspectStateConfig config;

        [Header("Controlled Containers")]
        [Tooltip("Drag AspectSnapshotContainer components here to register them with this switcher.")]
        public List<AspectSnapshotContainer> targets = new List<AspectSnapshotContainer>();

        [Header("Settings")]
        public TransitionSettings globalTransition = new TransitionSettings();
        [Tooltip("Seconds between checks. 0 = every frame.")]
        public float checkInterval = 0f;
        [Tooltip("New state must hold for this many seconds before switching. Prevents flicker at boundaries.")]
        public float stateStabilization = 0.05f;
        public bool applyOnStart = true;

        [Header("Events")]
        public AspectStateEvent onStateChanged;

        private float _timer;
        private AspectState? _pendingState;
        private float _pendingStateTime;

        private void Awake()
        {
            Instance = this;
            CurrentState = null;
        }

        private void Start()
        {
            AspectRatioMonitor.Reset();
            if (applyOnStart)
                EvaluateAndSwitch(forceApply: true);
        }

        private void Update()
        {
            if (checkInterval > 0f)
            {
                _timer += Time.deltaTime;
                if (_timer < checkInterval) return;
                _timer = 0f;
            }
            AspectRatioMonitor.Tick();
            EvaluateAndSwitch();
        }

        private void EvaluateAndSwitch(bool forceApply = false)
        {
            if (config == null) return;

            float aspect = AspectRatioMonitor.CurrentAspect;
            if (aspect <= 0f)
                aspect = Screen.width > 0 ? (float)Screen.width / Screen.height : 1f;

            AspectState? detected = config.FindState(aspect);
            if (detected == null) return;

            if (forceApply)
            {
                CurrentState   = detected;
                _pendingState  = null;
                _pendingStateTime = 0f;
                NotifyTargets(detected.Value);
                return;
            }

            if (detected == CurrentState)
            {
                _pendingState = null;
                return;
            }

            if (detected == _pendingState)
            {
                if (Time.unscaledTime - _pendingStateTime >= stateStabilization)
                {
                    CurrentState  = detected;
                    _pendingState = null;
                    NotifyTargets(detected.Value);
                }
            }
            else
            {
                _pendingState     = detected;
                _pendingStateTime = Time.unscaledTime;
            }
        }

        private void NotifyTargets(AspectState state)
        {
            for (int i = 0; i < targets.Count; i++)
            {
                if (targets[i] != null)
                    targets[i].HandleStateChanged(state);
            }
            OnStateChanged?.Invoke(state);
            onStateChanged?.Invoke(state);
        }

        public void ForceState(AspectState state)
        {
            CurrentState = state;
            NotifyTargets(state);
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
