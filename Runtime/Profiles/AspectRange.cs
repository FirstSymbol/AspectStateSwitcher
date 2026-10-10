using System;
using System.Globalization;
using UnityEngine;

namespace FirstSymbol.AspectRatioSwitcher
{
  /// <summary>
  /// Half-open range of aspect ratios (width / height): <c>min &lt;= aspect &lt; max</c>.
  /// A min of 0 means "no lower bound", a max of <see cref="Unbounded"/> means "no upper bound".
  /// </summary>
  [Serializable]
  public struct AspectRange : IEquatable<AspectRange>
  {
    public const float Unbounded = float.PositiveInfinity;

    /// <summary>Unbounded sides are clamped to this value when ranges are compared.</summary>
    public const float PracticalMax = 10f;

    public float min;
    public float max;

    public AspectRange(float min, float max)
    {
      this.min = Mathf.Max(0f, min);
      this.max = max;
    }

    public static AspectRange Any => new(0f, Unbounded);
    public static AspectRange Below(float max) => new(0f, max);
    public static AspectRange Above(float min) => new(min, Unbounded);
    public static AspectRange Between(float min, float max) => new(min, max);

    public bool HasLowerBound => min > 0f;
    public bool HasUpperBound => !float.IsPositiveInfinity(max);
    public bool IsValid => max > min;

    /// <summary>Range width used to decide which state is more specific.</summary>
    public float Span => Mathf.Min(max, PracticalMax) - Mathf.Min(min, PracticalMax);

    /// <summary>A representative aspect inside the range (geometric middle of the visible part).</summary>
    public float Center
    {
      get
      {
        float lo = Mathf.Max(min, 0.3f);
        float hi = Mathf.Min(max, 3.5f);
        return hi <= lo ? lo : Mathf.Sqrt(lo * hi);
      }
    }

    public bool Contains(float aspect) => aspect >= min && aspect < max;

    public bool Contains(AspectRange other) => other.min >= min && other.max <= max;

    public bool Overlaps(AspectRange other) => other.min < max && min < other.max;

    public bool Equals(AspectRange other) => min.Equals(other.min) && max.Equals(other.max);

    public override bool Equals(object obj) => obj is AspectRange other && Equals(other);

    public override int GetHashCode() => (min.GetHashCode() * 397) ^ max.GetHashCode();

    public override string ToString()
    {
      if (!HasLowerBound && !HasUpperBound)
        return "any";
      if (!HasLowerBound)
        return $"< {Format(max)}";
      if (!HasUpperBound)
        return $"≥ {Format(min)}";
      return $"{Format(min)} – {Format(max)}";
    }

    public static string Format(float aspect) => aspect.ToString("0.###", CultureInfo.InvariantCulture);
  }
}
