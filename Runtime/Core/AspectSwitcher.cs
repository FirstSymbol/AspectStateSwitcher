using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace FirstSymbol.AspectRatioSwitcher
{
  /// <summary>Raised with the name of the most specific active state.</summary>
  [Serializable]
  public sealed class AspectStateChangedEvent : UnityEvent<string>
  {
  }

  /// <summary>
  /// Measures an aspect ratio, resolves the active states of its <see cref="AspectProfile"/> and
  /// applies them to connected <see cref="AspectResponsive"/> components.
  /// </summary>
  [DisallowMultipleComponent]
  [DefaultExecutionOrder(-100)]
  [AddComponentMenu("Aspect Ratio Switcher/Aspect Switcher")]
  public sealed class AspectSwitcher : MonoBehaviour
  {
    public enum AspectSource
    {
      /// <summary>Screen.width / Screen.height.</summary>
      Screen,

      /// <summary>Pixel rect of a camera (Camera.main when not assigned).</summary>
      Camera,

      /// <summary>Size of a RectTransform, e.g. a panel or split-screen area.</summary>
      RectTransform
    }

    private const float AspectEpsilon = 0.0001f;
    private static readonly List<AspectSwitcher> Enabled = new();

    [SerializeField] private AspectProfile profile;
    [SerializeField] private AspectSource source = AspectSource.Screen;
    [SerializeField] private Camera targetCamera;
    [SerializeField] private RectTransform targetRect;

    [Tooltip("Seconds the aspect must stay unchanged before states switch (realtime). Avoids flicker while resizing.")]
    [SerializeField, Min(0f)] private float stabilizationTime = 0.1f;

    [SerializeField] private TransitionSettings defaultTransition = new();

    [Tooltip("Responsive components without a switcher in their parents use the main switcher.")]
    [SerializeField] private bool isMain = true;

    [SerializeField] private AspectStateChangedEvent onStateChanged = new();

    private readonly List<AspectResponsive> _responsives = new();
    private readonly List<AspectResponsive> _notifyBuffer = new();
    private readonly List<AspectStateDefinition> _activeStates = new();
    private readonly List<AspectStateDefinition> _scratchStates = new();
    private readonly List<AspectTween> _tweens = new();

    private float _aspect;
    private float _pendingAspect;
    private float _pendingSince;
    private bool _hasPending;
    private AspectStateDefinition _forcedState;

    /// <summary>The main switcher: the last enabled one marked as main, else the last enabled one.</summary>
    public static AspectSwitcher Main { get; private set; }

    public static event Action<AspectSwitcher> MainChanged;

    public AspectProfile Profile
    {
      get => profile;
      set
      {
        profile = value;
        if (isActiveAndEnabled)
          Refresh();
      }
    }

    public AspectSource Source
    {
      get => source;
      set => source = value;
    }

    public bool IsMain
    {
      get => isMain;
      set
      {
        isMain = value;
        if (isActiveAndEnabled)
          UpdateMain();
      }
    }

    public TransitionSettings DefaultTransition => defaultTransition;
    public AspectStateChangedEvent OnStateChanged => onStateChanged;

    /// <summary>Last committed aspect ratio.</summary>
    public float CurrentAspect => _aspect;

    /// <summary>Active states, most specific first.</summary>
    public IReadOnlyList<AspectStateDefinition> ActiveStates => _activeStates;

    public AspectStateDefinition PrimaryState => _activeStates.Count > 0 ? _activeStates[0] : null;
    public AspectStateDefinition ForcedState => _forcedState;
    public IReadOnlyList<AspectResponsive> Responsives => _responsives;
    public bool IsTransitioning => _tweens.Exists(tween => !tween.IsDone);

    public event Action<AspectSwitcher> StateChanged;

    private void OnEnable()
    {
      Enabled.Add(this);
      UpdateMain();
      _aspect = Measure();
      Evaluate(animate: false, force: true);
      AspectResponsive.ConnectWaiting();
    }

    private void OnDisable()
    {
      CompleteTweens();
      AspectResponsive[] connected = _responsives.ToArray();
      _responsives.Clear();

      Enabled.Remove(this);
      UpdateMain();

      // Connected components move to another switcher (or wait for one).
      foreach (AspectResponsive responsive in connected)
        responsive.OnSwitcherDisabled(this);
      AspectResponsive.ConnectWaiting();
    }

    private void Update()
    {
      TrackAspect();
      StepTweens();
    }

    /// <summary>Measures the aspect ratio of the configured source.</summary>
    public float Measure()
    {
      switch (source)
      {
        case AspectSource.Camera:
          Camera cam = targetCamera != null ? targetCamera : Camera.main;
          if (cam != null && cam.pixelHeight > 0)
            return (float)cam.pixelWidth / cam.pixelHeight;
          break;
        case AspectSource.RectTransform:
          if (targetRect != null && targetRect.rect.height > 0f)
            return targetRect.rect.width / targetRect.rect.height;
          break;
      }

      return UnityEngine.Screen.height > 0 ? (float)UnityEngine.Screen.width / UnityEngine.Screen.height : 1f;
    }

    /// <summary>Activates a state (and the broader states containing it) regardless of the aspect.</summary>
    public bool ForceState(string idOrName, bool animate = true)
    {
      AspectStateDefinition state = profile != null ? profile.Find(idOrName) : null;
      if (state == null)
      {
        Debug.LogWarning($"[AspectSwitcher] State '{idOrName}' is not defined in '{(profile != null ? profile.name : "no profile")}'.", this);
        return false;
      }

      _forcedState = state;
      Evaluate(animate, force: false);
      return true;
    }

    /// <summary>Returns to states resolved from the measured aspect.</summary>
    public void ClearForcedState(bool animate = true)
    {
      _forcedState = null;
      _aspect = Measure();
      Evaluate(animate, force: false);
    }

    /// <summary>Re-measures and re-applies the states to every connected component.</summary>
    public void Refresh(bool animate = false)
    {
      _aspect = Measure();
      Evaluate(animate, force: true);
    }

    /// <summary>Fills <paramref name="results"/> with the states that would be active for the aspect.</summary>
    public void GetStatesForAspect(float aspect, List<AspectStateDefinition> results)
    {
      if (profile != null)
        profile.GetMatchingStates(aspect, results);
      else
        results.Clear();
    }

    internal void Register(AspectResponsive responsive)
    {
      if (!_responsives.Contains(responsive))
        _responsives.Add(responsive);

      responsive.ResetAppliedStates();
      responsive.ApplyStates(_activeStates, null, null);
    }

    internal void Unregister(AspectResponsive responsive) => _responsives.Remove(responsive);

    private void TrackAspect()
    {
      if (_forcedState != null)
        return;

      float aspect = Measure();
      if (Mathf.Abs(aspect - _aspect) < AspectEpsilon)
      {
        _hasPending = false;
        return;
      }

      float now = Time.realtimeSinceStartup;
      if (!_hasPending || Mathf.Abs(aspect - _pendingAspect) >= AspectEpsilon)
      {
        _hasPending = true;
        _pendingAspect = aspect;
        _pendingSince = now;
      }

      if (now - _pendingSince < stabilizationTime)
        return;

      _hasPending = false;
      _aspect = aspect;
      Evaluate(animate: true, force: false);
    }

    private void Evaluate(bool animate, bool force)
    {
      _scratchStates.Clear();
      if (profile != null)
      {
        if (_forcedState != null)
          profile.GetFallbackChain(_forcedState, _scratchStates);
        else
          profile.GetMatchingStates(_aspect, _scratchStates);
      }

      bool changed = !SameStates(_scratchStates, _activeStates);
      if (!changed && !force)
        return;

      _activeStates.Clear();
      _activeStates.AddRange(_scratchStates);
      Notify(animate, force);

      if (changed)
      {
        StateChanged?.Invoke(this);
        onStateChanged?.Invoke(PrimaryState != null ? PrimaryState.name : string.Empty);
      }
    }

    private void Notify(bool animate, bool reapply)
    {
      _notifyBuffer.Clear();
      _notifyBuffer.AddRange(_responsives);

      foreach (AspectResponsive responsive in _notifyBuffer)
      {
        if (responsive == null || !responsive.isActiveAndEnabled)
          continue;

        if (reapply)
          responsive.ResetAppliedStates();
        responsive.ApplyStates(_activeStates, defaultTransition, animate ? _tweens : null);
      }

      _notifyBuffer.Clear();
    }

    private void StepTweens()
    {
      for (int i = _tweens.Count - 1; i >= 0; i--)
        if (!_tweens[i].Step())
          _tweens.RemoveAt(i);
    }

    private void CompleteTweens()
    {
      foreach (AspectTween tween in _tweens)
        tween.Complete();
      _tweens.Clear();
    }

    private static bool SameStates(List<AspectStateDefinition> a, List<AspectStateDefinition> b)
    {
      if (a.Count != b.Count)
        return false;
      for (int i = 0; i < a.Count; i++)
        if (!ReferenceEquals(a[i], b[i]))
          return false;
      return true;
    }

    private static void UpdateMain()
    {
      AspectSwitcher main = null;
      for (int i = Enabled.Count - 1; i >= 0; i--)
      {
        if (Enabled[i].isMain)
        {
          main = Enabled[i];
          break;
        }
      }

      if (main == null && Enabled.Count > 0)
        main = Enabled[Enabled.Count - 1];

      if (main == Main)
        return;

      Main = main;
      MainChanged?.Invoke(main);
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
      Enabled.Clear();
      Main = null;
      MainChanged = null;
    }
  }
}
