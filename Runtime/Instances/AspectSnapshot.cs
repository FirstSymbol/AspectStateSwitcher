using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AspectSwitcher
{
    public abstract class AspectSnapshot : MonoBehaviour
    {
        [Tooltip("The switcher that drives this snapshot. Self-registers on Enable.")]
        [SerializeField] private AspectRatioStateSwitcher _switcher;
        [SerializeField] private bool _workIfInactive = true;
        
        public Component target;
        public TransitionSettings transitionOverride = new TransitionSettings();

        public AspectRatioStateSwitcher Switcher => _switcher;

        private Coroutine   _transitionCoroutine;
        private AspectState? _currentAppliedState;

        public abstract ISnapshotData CreateSnapshotData();
        protected abstract ISnapshotData FindDataForState(AspectState state);
        public abstract ISnapshotData GetDataAt(int index);

        protected virtual Component FindDefaultTarget() => null;

        private void Reset()
        {
            var def = FindDefaultTarget();
            if (def != null) target = def;
        }

        private void Awake()
        {
            _currentAppliedState = null;
            _switcher?.Register(this);
        }

        private void OnDestroy()
        {
            _switcher?.Unregister(this);
        }

        private void OnEnable()
        {
            if (!_workIfInactive)
            {
                _currentAppliedState = null;
                _switcher?.Register(this);
            }
            
        }

        private void OnDisable()
        {
            if (!_workIfInactive)
            {
                _currentAppliedState = null;
                _switcher?.Unregister(this);
            }
        }

        public void HandleStateChanged(IReadOnlyList<AspectState> matchingStates)
        {
            ISnapshotData data         = null;
            AspectState   appliedState = default;

            for (int i = 0; i < matchingStates.Count; i++)
            {
                data = FindDataForState(matchingStates[i]);
                if (data != null) { appliedState = matchingStates[i]; break; }
            }

            if (data == null || _currentAppliedState == appliedState) return;

            _currentAppliedState = appliedState;

            var settings = ResolveTransition();
            if (_transitionCoroutine != null) StopCoroutine(_transitionCoroutine);

            if (settings == null || settings.mode == TransitionMode.Instant ||
                (!_workIfInactive && !gameObject.activeInHierarchy))
                data.ApplyTo(target, null, 1f);
            else
                _transitionCoroutine = StartCoroutine(TransitionCoroutine(data, settings));
        }

        private TransitionSettings ResolveTransition()
        {
            if (transitionOverride != null && transitionOverride.mode != TransitionMode.Instant)
                return transitionOverride;
            return _switcher?.globalTransition ?? AspectRatioStateSwitcher.Instance?.globalTransition;
        }

        private IEnumerator TransitionCoroutine(ISnapshotData toData, TransitionSettings settings)
        {
            var fromData = CreateSnapshotData();
            fromData?.CaptureFrom(target);

            float elapsed  = 0f;
            float duration = Mathf.Max(settings.duration, 0.001f);
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t01 = Mathf.Clamp01(elapsed / duration);
                float t   = settings.mode == TransitionMode.AnimationCurve && settings.curve != null
                    ? settings.curve.Evaluate(t01)
                    : t01;
                toData.ApplyTo(target, fromData, t);
                yield return null;
            }
            toData.ApplyTo(target, null, 1f);
            settings.onComplete?.Invoke();
            _transitionCoroutine = null;
        }

        public void ApplyStateInstant(AspectState state)
        {
            var data = FindDataForState(state);
            if (data == null) return;
            _currentAppliedState = state;
            data.ApplyTo(target, null, 1f);
        }

        public void ApplyStatesInstant(IReadOnlyList<AspectState> states)
        {
            for (int i = 0; i < states.Count; i++)
            {
                var data = FindDataForState(states[i]);
                if (data == null) continue;
                _currentAppliedState = states[i];
                data.ApplyTo(target, null, 1f);
                return;
            }
        }
    }
}
