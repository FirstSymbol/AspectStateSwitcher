using System;
using UnityEngine;

namespace FirstSymbol.AspectRatioSwitcher
{
  public enum TransitionMode
  {
    Instant,
    Tween
  }

  public enum TransitionOverride
  {
    /// <summary>Use the switcher's default transition.</summary>
    Inherit,
    Instant,
    Custom
  }

  [Serializable]
  public sealed class TransitionSettings
  {
    public TransitionMode mode = TransitionMode.Instant;
    [Min(0f)] public float duration = 0.3f;
    public AnimationCurve curve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Tooltip("Keep animating while Time.timeScale is 0.")]
    public bool unscaledTime = true;

    public bool IsAnimated => mode == TransitionMode.Tween && duration > 0f;

    public static TransitionSettings Tween(float duration) =>
      new() { mode = TransitionMode.Tween, duration = duration };
  }

  /// <summary>A running interpolation of one module, stepped by its switcher.</summary>
  public abstract class AspectTween
  {
    private readonly TransitionSettings _settings;
    private float _elapsed;

    protected AspectTween(TransitionSettings settings)
    {
      _settings = settings;
    }

    public bool IsDone { get; private set; }

    /// <summary>Advances the tween. Returns false once finished.</summary>
    public bool Step()
    {
      if (IsDone)
        return false;

      _elapsed += _settings.unscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
      float progress = _settings.duration > 0f ? Mathf.Clamp01(_elapsed / _settings.duration) : 1f;
      if (progress >= 1f)
      {
        Complete();
        return false;
      }

      Evaluate(_settings.curve != null ? _settings.curve.Evaluate(progress) : progress);
      return true;
    }

    /// <summary>Jumps to the end value.</summary>
    public void Complete()
    {
      if (IsDone)
        return;

      IsDone = true;
      Evaluate(1f);
    }

    /// <summary>Stops without applying the end value.</summary>
    public void Cancel() => IsDone = true;

    protected abstract void Evaluate(float t);
  }
}
