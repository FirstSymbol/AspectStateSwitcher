using System;
using System.Collections;
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

        [Header("Transition")]
        public TransitionSettings globalTransition = new TransitionSettings();
        [Tooltip("New state must hold for this many seconds before switching. Prevents flicker at boundaries.")]
        public float stateStabilization = 0.05f;
        public bool applyOnStart = true;

        [Header("Events")]
        public AspectStateEvent onStateChanged;

        // Containers keyed by SnapshotType for O(1) grouped access.
        // Populated at runtime via AspectSnapshotContainer.OnEnable / OnDisable.
        private readonly Dictionary<SnapshotType, List<AspectSnapshotContainer>> _containers
            = new Dictionary<SnapshotType, List<AspectSnapshotContainer>>();

        private AspectState? _pendingState;
        private Coroutine _stabilizationRoutine;

        private void Awake()
        {
            Instance    = this;
            CurrentState = null;
        }

        private void OnEnable()  => AspectRatioMonitor.OnAspectChanged += HandleAspectChanged;
        private void OnDisable() => AspectRatioMonitor.OnAspectChanged -= HandleAspectChanged;

        private void Start()
        {
            AspectRatioMonitor.Initialize();
            if (applyOnStart) EvaluateAndSwitch(forceApply: true);
        }

        private void HandleAspectChanged(float _) => EvaluateAndSwitch();

        // ── Container registration ────────────────────────────────────────────────

        public void Register(AspectSnapshotContainer c)
        {
            if (c == null) return;
            if (!_containers.TryGetValue(c.type, out var list))
                _containers[c.type] = list = new List<AspectSnapshotContainer>();
            if (!list.Contains(c)) list.Add(c);
        }

        public void Unregister(AspectSnapshotContainer c)
        {
            if (c != null && _containers.TryGetValue(c.type, out var list))
                list.Remove(c);
        }

        public IReadOnlyDictionary<SnapshotType, List<AspectSnapshotContainer>> RegisteredContainers
            => _containers;

        // ── State logic ───────────────────────────────────────────────────────────

        private void EvaluateAndSwitch(bool forceApply = false)
        {
            if (config == null) return;

            float aspect = AspectRatioMonitor.CurrentAspect;
            if (aspect <= 0f)
                aspect = Screen.width > 0 ? (float)Screen.width / Screen.height : 1f;

            var detected = config.FindState(aspect);
            if (detected == null) return;

            if (forceApply)
            {
                CancelStabilization();
                CurrentState  = detected;
                _pendingState = null;
                NotifyContainers(detected.Value);
                return;
            }

            if (detected == CurrentState)
            {
                CancelStabilization();
                _pendingState = null;
                return;
            }

            // New state already pending — stabilization coroutine is already running.
            if (detected == _pendingState) return;

            // Different pending state: restart stabilization.
            _pendingState = detected;
            CancelStabilization();

            if (stateStabilization > 0f)
                _stabilizationRoutine = StartCoroutine(StabilizeAndCommit(detected.Value, stateStabilization));
            else
                CommitState(detected.Value);
        }

        private IEnumerator StabilizeAndCommit(AspectState target, float delay)
        {
            yield return new WaitForSecondsRealtime(delay);
            _stabilizationRoutine = null;
            if (_pendingState == target) CommitState(target);
        }

        private void CommitState(AspectState state)
        {
            CurrentState  = state;
            _pendingState = null;
            NotifyContainers(state);
        }

        private void CancelStabilization()
        {
            if (_stabilizationRoutine == null) return;
            StopCoroutine(_stabilizationRoutine);
            _stabilizationRoutine = null;
        }

        private void NotifyContainers(AspectState state)
        {
            foreach (var list in _containers.Values)
                for (int i = 0; i < list.Count; i++)
                    if (list[i] != null) list[i].HandleStateChanged(state);

            OnStateChanged?.Invoke(state);
            onStateChanged?.Invoke(state);
        }

        public void ForceState(AspectState state)
        {
            CancelStabilization();
            CurrentState  = state;
            _pendingState = null;
            NotifyContainers(state);
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
