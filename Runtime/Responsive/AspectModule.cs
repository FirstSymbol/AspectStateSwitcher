using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace FirstSymbol.AspectRatioSwitcher
{
  /// <summary>Menu path of a module in the "Add Module" dropdown, e.g. "Layout/Grid Layout Group".</summary>
  [AttributeUsage(AttributeTargets.Class, Inherited = false)]
  public sealed class AspectModuleMenuAttribute : Attribute
  {
    public AspectModuleMenuAttribute(string path)
    {
      Path = path;
    }

    public string Path { get; }
  }

  /// <summary>
  /// Stores values of one target per aspect state and applies the value of the active state.
  /// Derive from <see cref="AspectModule{TTarget, TValue}"/> to add new module types.
  /// </summary>
  [Serializable]
  public abstract class AspectModule
  {
    [SerializeField] private bool enabled = true;

    [NonSerialized] private string _appliedStateId;
    [NonSerialized] private AspectTween _tween;

    public bool Enabled
    {
      get => enabled;
      set => enabled = value;
    }

    /// <summary>State whose value was applied last (runtime only).</summary>
    public string AppliedStateId => _appliedStateId;

    public abstract Object Target { get; set; }
    public abstract Type TargetType { get; }
    public abstract IEnumerable<string> StateIds { get; }

    public virtual bool SupportsInterpolation => false;

    public string DisplayName
    {
      get
      {
        var menu = (AspectModuleMenuAttribute)Attribute.GetCustomAttribute(GetType(), typeof(AspectModuleMenuAttribute));
        if (menu == null)
          return GetType().Name;
        int slash = menu.Path.LastIndexOf('/');
        return slash >= 0 ? menu.Path.Substring(slash + 1) : menu.Path;
      }
    }

    public abstract bool HasValue(string stateId);

    /// <summary>Stores the current target values as the value of the state.</summary>
    public abstract void Capture(string stateId);

    public abstract bool Clear(string stateId);

    /// <summary>Applies the value of the state immediately.</summary>
    public abstract void ApplyInstant(string stateId);

    /// <summary>Removes values of states that do not exist in the profile.</summary>
    public abstract int RemoveUnknownStates(AspectProfile profile);

    /// <summary>Assigns a sensible target from the owner object when none is set.</summary>
    public virtual void AssignDefaultTarget(GameObject owner) { }

    /// <summary>A configuration problem to show in the inspector, or null.</summary>
    public virtual string GetWarning(AspectResponsive owner) =>
      Target == null ? "Target is not assigned." : null;

    protected abstract AspectTween CreateTween(string stateId, TransitionSettings settings);

    /// <summary>Current target values, boxed (used by editor previews to restore the scene).</summary>
    internal abstract object ReadCurrent();

    internal abstract void WriteCurrent(object value);

    internal void Apply(string stateId, TransitionSettings transition, List<AspectTween> tweens)
    {
      if (stateId == _appliedStateId)
        return;

      FinishTween();
      _appliedStateId = stateId;

      if (tweens != null && transition != null && transition.IsAnimated && SupportsInterpolation && Target != null)
      {
        _tween = CreateTween(stateId, transition);
        if (_tween != null)
        {
          tweens.Add(_tween);
          return;
        }
      }

      ApplyInstant(stateId);
    }

    internal void FinishTween()
    {
      _tween?.Complete();
      _tween = null;
    }

    internal void ResetAppliedState()
    {
      FinishTween();
      _appliedStateId = null;
    }
  }

  /// <summary>Value of a module for one state.</summary>
  [Serializable]
  public struct AspectStateValue<TValue>
  {
    public string stateId;
    public TValue value;
  }

  /// <summary>
  /// Module with a typed target and value. Implement <see cref="Read"/> and <see cref="Write"/>;
  /// override <see cref="Interpolate"/> and <see cref="SupportsInterpolation"/> to support tweens.
  /// </summary>
  [Serializable]
  public abstract class AspectModule<TTarget, TValue> : AspectModule where TTarget : Object
  {
    [SerializeField] protected TTarget target;
    [SerializeField] private List<AspectStateValue<TValue>> values = new();

    public override Object Target
    {
      get => target;
      set => target = value as TTarget;
    }

    public TTarget TypedTarget
    {
      get => target;
      set => target = value;
    }

    public override Type TargetType => typeof(TTarget);

    public override IEnumerable<string> StateIds
    {
      get
      {
        foreach (AspectStateValue<TValue> entry in values)
          yield return entry.stateId;
      }
    }

    public override bool HasValue(string stateId) => IndexOf(stateId) >= 0;

    public bool TryGetValue(string stateId, out TValue value)
    {
      int index = IndexOf(stateId);
      value = index >= 0 ? values[index].value : default;
      return index >= 0;
    }

    public void SetValue(string stateId, TValue value)
    {
      var entry = new AspectStateValue<TValue> { stateId = stateId, value = value };
      int index = IndexOf(stateId);
      if (index >= 0)
        values[index] = entry;
      else
        values.Add(entry);
    }

    public override void Capture(string stateId)
    {
      if (target != null)
        SetValue(stateId, Read(target));
    }

    public override bool Clear(string stateId)
    {
      int index = IndexOf(stateId);
      if (index < 0)
        return false;

      values.RemoveAt(index);
      return true;
    }

    public override void ApplyInstant(string stateId)
    {
      if (target != null && TryGetValue(stateId, out TValue value))
        Write(target, value);
    }

    public override int RemoveUnknownStates(AspectProfile profile) =>
      profile == null ? 0 : values.RemoveAll(entry => !profile.TryGet(entry.stateId, out _));

    public override void AssignDefaultTarget(GameObject owner)
    {
      if (target != null || owner == null)
        return;

      if (typeof(TTarget) == typeof(GameObject))
        target = owner as TTarget;
      else if (typeof(Component).IsAssignableFrom(typeof(TTarget)) && typeof(TTarget) != typeof(Component))
        target = owner.GetComponent(typeof(TTarget)) as TTarget;
    }

    protected abstract TValue Read(TTarget target);
    protected abstract void Write(TTarget target, TValue value);

    internal override object ReadCurrent() => target != null ? Read(target) : null;

    internal override void WriteCurrent(object value)
    {
      if (target != null && value is TValue typed)
        Write(target, typed);
    }

    /// <summary>Value between <paramref name="from"/> and <paramref name="to"/>. Discrete values should switch to <paramref name="to"/>.</summary>
    protected virtual TValue Interpolate(TValue from, TValue to, float t) => to;

    protected override AspectTween CreateTween(string stateId, TransitionSettings settings)
    {
      if (target == null || !TryGetValue(stateId, out TValue to))
        return null;
      return new Tween(this, Read(target), to, settings);
    }

    private int IndexOf(string stateId)
    {
      for (int i = 0; i < values.Count; i++)
        if (values[i].stateId == stateId)
          return i;
      return -1;
    }

    private sealed class Tween : AspectTween
    {
      private readonly AspectModule<TTarget, TValue> _module;
      private readonly TValue _from;
      private readonly TValue _to;

      public Tween(AspectModule<TTarget, TValue> module, TValue from, TValue to, TransitionSettings settings)
        : base(settings)
      {
        _module = module;
        _from = from;
        _to = to;
      }

      protected override void Evaluate(float t)
      {
        if (_module.target != null)
          _module.Write(_module.target, t >= 1f ? _to : _module.Interpolate(_from, _to, t));
      }
    }
  }
}
