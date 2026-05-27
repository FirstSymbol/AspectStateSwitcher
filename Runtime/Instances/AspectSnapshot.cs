using System.Collections;
using UnityEngine;

namespace AspectSwitcher
{
    public abstract class AspectSnapshot : MonoBehaviour
    {
        [Tooltip("The switcher that drives this snapshot. Self-registers on Enable.")]
        [SerializeField] private AspectRatioStateSwitcher _switcher;

        public Component target;
        public TransitionSettings transitionOverride = new TransitionSettings();

        public AspectRatioStateSwitcher Switcher => _switcher;

        private Coroutine _transitionCoroutine;

        public abstract ISnapshotData CreateSnapshotData();
        protected abstract ISnapshotData FindDataForState(AspectState state);
        public abstract ISnapshotData GetDataAt(int index);

        protected virtual Component FindDefaultTarget() => null;

        private void Reset()
        {
            var def = FindDefaultTarget();
            if (def != null) target = def;
        }

        private void OnEnable()  => _switcher?.Register(this);
        private void OnDisable() => _switcher?.Unregister(this);

        public void HandleStateChanged(AspectState state)
        {
            var data = FindDataForAspect() ?? FindDataForState(state);
            if (data == null) return;

            var settings = ResolveTransition();

            if (_transitionCoroutine != null) StopCoroutine(_transitionCoroutine);

            if (settings == null || settings.mode == TransitionMode.Instant || !gameObject.activeInHierarchy)
                data.ApplyTo(target, null, 1f);
            else
                _transitionCoroutine = StartCoroutine(TransitionCoroutine(data, settings));
        }

        private ISnapshotData FindDataForAspect()
        {
            var config = _switcher?.config;
            if (config == null) return null;

            float aspect = AspectRatioMonitor.CurrentAspect;
            for (int i = 0; i < config.states.Count; i++)
            {
                if (!config.states[i].range.Matches(aspect)) continue;
                var data = FindDataForState(config.states[i].state);
                if (data != null) return data;
            }
            return null;
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
            FindDataForState(state)?.ApplyTo(target, null, 1f);
        }
    }
}
