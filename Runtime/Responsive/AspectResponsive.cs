using System;
using System.Collections.Generic;
using UnityEngine;

namespace FirstSymbol.AspectRatioSwitcher
{
  /// <summary>
  /// Holds the modules of an object and applies them when the active aspect states change.
  /// Connects to the explicit switcher, else to the nearest switcher in parents, else to <see cref="AspectSwitcher.Main"/>.
  /// </summary>
  [DisallowMultipleComponent]
  [AddComponentMenu("Aspect Ratio Switcher/Aspect Responsive")]
  public sealed class AspectResponsive : MonoBehaviour
  {
    private static readonly List<AspectResponsive> Waiting = new();

    [Tooltip("Optional. Empty = nearest switcher in parents, then the main switcher.")]
    [SerializeField] private AspectSwitcher switcher;

    [SerializeField] private TransitionOverride transition = TransitionOverride.Inherit;
    [SerializeField] private TransitionSettings customTransition = TransitionSettings.Tween(0.3f);

    [SerializeReference] private List<AspectModule> modules = new();

    [NonSerialized] private AspectSwitcher _connected;

    public IReadOnlyList<AspectModule> Modules => modules;

    /// <summary>Switcher this component is connected to at runtime.</summary>
    public AspectSwitcher ConnectedSwitcher => _connected;

    public AspectSwitcher ExplicitSwitcher
    {
      get => switcher;
      set
      {
        switcher = value;
        if (isActiveAndEnabled)
        {
          Disconnect();
          Connect();
        }
      }
    }

    public TransitionOverride Transition
    {
      get => transition;
      set => transition = value;
    }

    public TransitionSettings CustomTransition => customTransition;

    private void OnEnable() => Connect();

    private void OnDisable()
    {
      Disconnect();
      foreach (AspectModule module in modules)
        module?.FinishTween();
    }

    /// <summary>Enabled switcher this component would use: explicit, parent, then main.</summary>
    public AspectSwitcher ResolveSwitcher()
    {
      if (switcher != null)
        return switcher.isActiveAndEnabled ? switcher : null;

      AspectSwitcher parent = GetComponentInParent<AspectSwitcher>();
      return parent != null && parent.isActiveAndEnabled ? parent : AspectSwitcher.Main;
    }

    public T AddModule<T>() where T : AspectModule, new() => (T)AddModule(typeof(T));

    public AspectModule AddModule(Type moduleType)
    {
      if (moduleType == null || !typeof(AspectModule).IsAssignableFrom(moduleType) || moduleType.IsAbstract)
        throw new ArgumentException($"{moduleType} is not a concrete AspectModule type.", nameof(moduleType));

      var module = (AspectModule)Activator.CreateInstance(moduleType);
      module.AssignDefaultTarget(gameObject);
      modules.Add(module);
      return module;
    }

    public bool RemoveModule(AspectModule module) => modules.Remove(module);

    public bool HasAnyValue(string stateId)
    {
      foreach (AspectModule module in modules)
        if (module != null && module.HasValue(stateId))
          return true;
      return false;
    }

    /// <summary>Captures the current values of every module into the state.</summary>
    public void CaptureAll(string stateId)
    {
      foreach (AspectModule module in modules)
        if (module != null && module.Target != null)
          module.Capture(stateId);
    }

    public void ClearState(string stateId)
    {
      foreach (AspectModule module in modules)
        module?.Clear(stateId);
    }

    /// <summary>Applies prioritized states (most specific first) immediately, e.g. for previews.</summary>
    public void ApplyStatesInstant(IReadOnlyList<AspectStateDefinition> prioritized)
    {
      ResetAppliedStates();
      ApplyStates(prioritized, null, null);
    }

    internal void ApplyStates(IReadOnlyList<AspectStateDefinition> prioritized, TransitionSettings defaultTransition,
      List<AspectTween> tweens)
    {
      TransitionSettings settings = transition switch
      {
        TransitionOverride.Instant => null,
        TransitionOverride.Custom => customTransition,
        _ => defaultTransition
      };

      foreach (AspectModule module in modules)
      {
        if (module == null || !module.Enabled)
          continue;

        AspectStateDefinition state = AspectStateResolver.Resolve(prioritized, module.HasValue);
        if (state != null)
          module.Apply(state.Id, settings, tweens);
      }
    }

    internal void ResetAppliedStates()
    {
      foreach (AspectModule module in modules)
        module?.ResetAppliedState();
    }

    internal static void ConnectWaiting()
    {
      if (Waiting.Count == 0)
        return;

      foreach (AspectResponsive responsive in Waiting.ToArray())
        if (responsive != null && responsive.isActiveAndEnabled && responsive._connected == null)
          responsive.Connect();
    }

    private void Connect()
    {
      AspectSwitcher target = ResolveSwitcher();
      if (target == null)
      {
        if (!Waiting.Contains(this))
          Waiting.Add(this);
        return;
      }

      Waiting.Remove(this);
      _connected = target;
      target.Register(this);
    }

    internal void OnSwitcherDisabled(AspectSwitcher disabled)
    {
      if (_connected != disabled)
        return;

      _connected = null;
      if (isActiveAndEnabled && !Waiting.Contains(this))
        Waiting.Add(this);
    }

    internal void Disconnect()
    {
      Waiting.Remove(this);
      if (_connected != null)
        _connected.Unregister(this);
      _connected = null;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() => Waiting.Clear();
  }
}
