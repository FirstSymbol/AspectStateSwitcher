using System;
using UnityEngine;
using UnityEngine.Events;

namespace FirstSymbol.AspectRatioSwitcher.Modules
{
  /// <summary>Activates or deactivates a GameObject.</summary>
  [Serializable, AspectModuleMenu("Object/GameObject Active")]
  public sealed class GameObjectActiveModule : AspectModule<GameObject, bool>
  {
    protected override bool Read(GameObject target) => target.activeSelf;

    protected override void Write(GameObject target, bool value)
    {
      if (target.activeSelf != value)
        target.SetActive(value);
    }

    public override void AssignDefaultTarget(GameObject owner)
    {
      // The owner itself is a bad default: deactivating it stops the component.
    }

    public override string GetWarning(AspectResponsive owner)
    {
      string warning = base.GetWarning(owner);
      if (warning != null)
        return warning;
      return target == owner.gameObject
        ? "Deactivating the object that owns this component stops it from reacting. Put the component on a parent."
        : null;
    }
  }

  /// <summary>Enables or disables a Behaviour, Renderer or Collider.</summary>
  [Serializable, AspectModuleMenu("Object/Component Enabled")]
  public sealed class ComponentEnabledModule : AspectModule<Component, bool>
  {
    protected override bool Read(Component target) => target switch
    {
      Behaviour behaviour => behaviour.enabled,
      Renderer renderer => renderer.enabled,
      Collider collider => collider.enabled,
      _ => true
    };

    protected override void Write(Component target, bool value)
    {
      switch (target)
      {
        case Behaviour behaviour:
          behaviour.enabled = value;
          break;
        case Renderer renderer:
          renderer.enabled = value;
          break;
        case Collider collider:
          collider.enabled = value;
          break;
      }
    }

    public override string GetWarning(AspectResponsive owner)
    {
      string warning = base.GetWarning(owner);
      if (warning != null)
        return warning;
      if (target is not (Behaviour or Renderer or Collider))
        return $"{target.GetType().Name} cannot be enabled or disabled.";
      return target == owner ? "This module cannot target the Aspect Responsive component itself." : null;
    }
  }

  [Serializable]
  public struct AnimatorParameterValue
  {
    public float number;
    public bool flag;
  }

  /// <summary>Sets an Animator parameter. Triggers fire when the state becomes active and the flag is set.</summary>
  [Serializable, AspectModuleMenu("Object/Animator Parameter")]
  public sealed class AnimatorParameterModule : AspectModule<Animator, AnimatorParameterValue>
  {
    public string parameterName;
    public AnimatorControllerParameterType parameterType = AnimatorControllerParameterType.Float;

    [NonSerialized] private int _hash;
    [NonSerialized] private string _hashedName;

    public override bool SupportsInterpolation => parameterType == AnimatorControllerParameterType.Float;

    protected override AnimatorParameterValue Read(Animator animator)
    {
      if (!CanAccess(animator))
        return default;

      int hash = Hash;
      return parameterType switch
      {
        AnimatorControllerParameterType.Float => new AnimatorParameterValue { number = animator.GetFloat(hash) },
        AnimatorControllerParameterType.Int => new AnimatorParameterValue { number = animator.GetInteger(hash) },
        AnimatorControllerParameterType.Bool => new AnimatorParameterValue { flag = animator.GetBool(hash) },
        _ => default
      };
    }

    protected override void Write(Animator animator, AnimatorParameterValue value)
    {
      if (!CanAccess(animator))
        return;

      int hash = Hash;
      switch (parameterType)
      {
        case AnimatorControllerParameterType.Float:
          animator.SetFloat(hash, value.number);
          break;
        case AnimatorControllerParameterType.Int:
          animator.SetInteger(hash, Mathf.RoundToInt(value.number));
          break;
        case AnimatorControllerParameterType.Bool:
          animator.SetBool(hash, value.flag);
          break;
        case AnimatorControllerParameterType.Trigger:
          if (value.flag)
            animator.SetTrigger(hash);
          break;
      }
    }

    protected override AnimatorParameterValue Interpolate(AnimatorParameterValue a, AnimatorParameterValue b, float t) =>
      new() { number = Mathf.LerpUnclamped(a.number, b.number, t), flag = b.flag };

    public override string GetWarning(AspectResponsive owner) =>
      base.GetWarning(owner) ?? (string.IsNullOrEmpty(parameterName) ? "Parameter name is empty." : null);

    private int Hash
    {
      get
      {
        if (_hashedName != parameterName)
        {
          _hashedName = parameterName;
          _hash = Animator.StringToHash(parameterName ?? string.Empty);
        }

        return _hash;
      }
    }

    private bool CanAccess(Animator animator) =>
      !string.IsNullOrEmpty(parameterName) && animator.isActiveAndEnabled && animator.runtimeAnimatorController != null;
  }

  /// <summary>Invokes a UnityEvent when the state becomes active. Useful for anything without a dedicated module.</summary>
  [Serializable, AspectModuleMenu("Object/State Events")]
  public sealed class StateEventsModule : AspectModule<GameObject, UnityEvent>
  {
    protected override UnityEvent Read(GameObject target) => new();

    protected override void Write(GameObject target, UnityEvent value) => value?.Invoke();

    /// <summary>Adds an empty event for the state without overwriting an existing one.</summary>
    public override void Capture(string stateId)
    {
      if (!HasValue(stateId))
        SetValue(stateId, new UnityEvent());
    }

    public override void ApplyInstant(string stateId)
    {
      if (TryGetValue(stateId, out UnityEvent value))
        value?.Invoke();
    }

    public override void AssignDefaultTarget(GameObject owner) => target = owner;

    public override string GetWarning(AspectResponsive owner) => null;
  }
}
