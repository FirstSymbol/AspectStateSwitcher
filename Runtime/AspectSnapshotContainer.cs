using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AspectSwitcher
{
    public class AspectSnapshotContainer : MonoBehaviour
    {
        [Tooltip("Config that defines valid states. Must be assigned before adding entries.")]
        public AspectStateConfig config;

        public SnapshotType type;
        public Component target;
        public List<StateEntry> entries = new List<StateEntry>();
        public TransitionSettings transitionOverride = new TransitionSettings();

        private Coroutine _transitionCoroutine;

        public void HandleStateChanged(AspectState state)
        {
            if (config == null) return;

            var entry = entries.Find(e => e.state == state);
            if (entry?.data == null) return;

            var settings = ResolveTransition();

            if (_transitionCoroutine != null)
                StopCoroutine(_transitionCoroutine);

            if (settings == null || settings.mode == TransitionMode.Instant || !gameObject.activeInHierarchy)
                entry.data.ApplyTo(target, null, 1f);
            else
                _transitionCoroutine = StartCoroutine(TransitionCoroutine(entry.data, settings));
        }

        private TransitionSettings ResolveTransition()
        {
            if (transitionOverride != null && transitionOverride.mode != TransitionMode.Instant)
                return transitionOverride;
            return AspectRatioStateSwitcher.Instance?.globalTransition;
        }

        private IEnumerator TransitionCoroutine(ISnapshotData toData, TransitionSettings settings)
        {
            var fromData = type.CreateData();
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
            var entry = entries.Find(e => e.state == state);
            entry?.data?.ApplyTo(target, null, 1f);
        }

    }
}
