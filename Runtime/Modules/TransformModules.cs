using System;
using UnityEngine;

namespace FirstSymbol.AspectRatioSwitcher.Modules
{
  [Flags]
  public enum RectTransformProperties
  {
    AnchoredPosition = 1 << 0,
    SizeDelta = 1 << 1,
    Anchors = 1 << 2,
    Pivot = 1 << 3,
    Rotation = 1 << 4,
    Scale = 1 << 5,
    All = AnchoredPosition | SizeDelta | Anchors | Pivot | Rotation | Scale
  }

  [Serializable]
  public struct RectTransformValues
  {
    public Vector2 anchorMin;
    public Vector2 anchorMax;
    public Vector2 pivot;
    public Vector2 anchoredPosition;
    public Vector2 sizeDelta;
    public Vector3 localEulerAngles;
    public Vector3 localScale;
  }

  /// <summary>Layout of a RectTransform: position, size, anchors, pivot, rotation and scale.</summary>
  [Serializable, AspectModuleMenu("Transform/Rect Transform")]
  public sealed class RectTransformModule : AspectModule<RectTransform, RectTransformValues>
  {
    [Tooltip("Properties written by this module.")]
    public RectTransformProperties properties = RectTransformProperties.All;

    public override bool SupportsInterpolation => true;

    protected override RectTransformValues Read(RectTransform t) => new()
    {
      anchorMin = t.anchorMin,
      anchorMax = t.anchorMax,
      pivot = t.pivot,
      anchoredPosition = t.anchoredPosition,
      sizeDelta = t.sizeDelta,
      localEulerAngles = t.localEulerAngles,
      localScale = t.localScale
    };

    protected override void Write(RectTransform t, RectTransformValues v)
    {
      if (Has(RectTransformProperties.Anchors))
      {
        t.anchorMin = v.anchorMin;
        t.anchorMax = v.anchorMax;
      }

      if (Has(RectTransformProperties.Pivot))
        t.pivot = v.pivot;
      if (Has(RectTransformProperties.SizeDelta))
        t.sizeDelta = v.sizeDelta;
      if (Has(RectTransformProperties.AnchoredPosition))
        t.anchoredPosition = v.anchoredPosition;
      if (Has(RectTransformProperties.Rotation))
        t.localEulerAngles = v.localEulerAngles;
      if (Has(RectTransformProperties.Scale))
        t.localScale = v.localScale;
    }

    protected override RectTransformValues Interpolate(RectTransformValues a, RectTransformValues b, float t) => new()
    {
      anchorMin = Vector2.LerpUnclamped(a.anchorMin, b.anchorMin, t),
      anchorMax = Vector2.LerpUnclamped(a.anchorMax, b.anchorMax, t),
      pivot = Vector2.LerpUnclamped(a.pivot, b.pivot, t),
      anchoredPosition = Vector2.LerpUnclamped(a.anchoredPosition, b.anchoredPosition, t),
      sizeDelta = Vector2.LerpUnclamped(a.sizeDelta, b.sizeDelta, t),
      localEulerAngles = Quaternion.SlerpUnclamped(Quaternion.Euler(a.localEulerAngles), Quaternion.Euler(b.localEulerAngles), t).eulerAngles,
      localScale = Vector3.LerpUnclamped(a.localScale, b.localScale, t)
    };

    private bool Has(RectTransformProperties flag) => (properties & flag) != 0;
  }

  [Flags]
  public enum TransformProperties
  {
    Position = 1 << 0,
    Rotation = 1 << 1,
    Scale = 1 << 2,
    All = Position | Rotation | Scale
  }

  [Serializable]
  public struct TransformValues
  {
    public Vector3 localPosition;
    public Vector3 localEulerAngles;
    public Vector3 localScale;
  }

  /// <summary>Local position, rotation and scale of any Transform.</summary>
  [Serializable, AspectModuleMenu("Transform/Transform")]
  public sealed class TransformModule : AspectModule<Transform, TransformValues>
  {
    public TransformProperties properties = TransformProperties.All;

    public override bool SupportsInterpolation => true;

    protected override TransformValues Read(Transform t) => new()
    {
      localPosition = t.localPosition,
      localEulerAngles = t.localEulerAngles,
      localScale = t.localScale
    };

    protected override void Write(Transform t, TransformValues v)
    {
      if ((properties & TransformProperties.Position) != 0)
        t.localPosition = v.localPosition;
      if ((properties & TransformProperties.Rotation) != 0)
        t.localEulerAngles = v.localEulerAngles;
      if ((properties & TransformProperties.Scale) != 0)
        t.localScale = v.localScale;
    }

    protected override TransformValues Interpolate(TransformValues a, TransformValues b, float t) => new()
    {
      localPosition = Vector3.LerpUnclamped(a.localPosition, b.localPosition, t),
      localEulerAngles = Quaternion.SlerpUnclamped(Quaternion.Euler(a.localEulerAngles), Quaternion.Euler(b.localEulerAngles), t).eulerAngles,
      localScale = Vector3.LerpUnclamped(a.localScale, b.localScale, t)
    };
  }

  /// <summary>Position of the object among its siblings (affects draw order and layout order).</summary>
  [Serializable, AspectModuleMenu("Transform/Sibling Index")]
  public sealed class SiblingIndexModule : AspectModule<Transform, int>
  {
    protected override int Read(Transform t) => t.GetSiblingIndex();
    protected override void Write(Transform t, int value) => t.SetSiblingIndex(value);
  }

  /// <summary>Moves the object under another parent. Keep parents inside the same hierarchy or prefab.</summary>
  [Serializable, AspectModuleMenu("Transform/Parent")]
  public sealed class ParentModule : AspectModule<Transform, Transform>
  {
    [Tooltip("Keep the world position when the parent changes. Usually off for UI.")]
    public bool worldPositionStays;

    protected override Transform Read(Transform t) => t.parent;

    protected override void Write(Transform t, Transform parent)
    {
      if (t.parent != parent)
        t.SetParent(parent, worldPositionStays);
    }

    public override string GetWarning(AspectResponsive owner)
    {
      string warning = base.GetWarning(owner);
      if (warning != null)
        return warning;
      return target == owner.transform
        ? "Reparenting the object that owns this component may disconnect it from its switcher."
        : null;
    }
  }
}
