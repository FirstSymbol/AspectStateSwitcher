using System;
using UnityEngine;
using UnityEngine.UI;

namespace FirstSymbol.AspectRatioSwitcher.Modules
{
  /// <summary>Serializable copy of <see cref="RectOffset"/>.</summary>
  [Serializable]
  public struct LayoutPadding
  {
    public int left;
    public int right;
    public int top;
    public int bottom;

    public static LayoutPadding From(RectOffset offset) => offset == null
      ? default
      : new LayoutPadding { left = offset.left, right = offset.right, top = offset.top, bottom = offset.bottom };

    public RectOffset ToRectOffset() => new(left, right, top, bottom);

    public static LayoutPadding Lerp(LayoutPadding a, LayoutPadding b, float t) => new()
    {
      left = Mathf.RoundToInt(Mathf.LerpUnclamped(a.left, b.left, t)),
      right = Mathf.RoundToInt(Mathf.LerpUnclamped(a.right, b.right, t)),
      top = Mathf.RoundToInt(Mathf.LerpUnclamped(a.top, b.top, t)),
      bottom = Mathf.RoundToInt(Mathf.LerpUnclamped(a.bottom, b.bottom, t))
    };
  }

  [Serializable]
  public struct LayoutGroupValues
  {
    public LayoutPadding padding;
    public float spacing;
    public TextAnchor childAlignment;
    public bool reverseArrangement;
    public bool controlChildWidth;
    public bool controlChildHeight;
    public bool useChildScaleWidth;
    public bool useChildScaleHeight;
    public bool forceExpandWidth;
    public bool forceExpandHeight;
  }

  /// <summary>Settings of a Horizontal or Vertical Layout Group.</summary>
  [Serializable, AspectModuleMenu("Layout/Horizontal or Vertical Layout Group")]
  public sealed class LayoutGroupModule : AspectModule<HorizontalOrVerticalLayoutGroup, LayoutGroupValues>
  {
    public override bool SupportsInterpolation => true;

    protected override LayoutGroupValues Read(HorizontalOrVerticalLayoutGroup g) => new()
    {
      padding = LayoutPadding.From(g.padding),
      spacing = g.spacing,
      childAlignment = g.childAlignment,
      reverseArrangement = g.reverseArrangement,
      controlChildWidth = g.childControlWidth,
      controlChildHeight = g.childControlHeight,
      useChildScaleWidth = g.childScaleWidth,
      useChildScaleHeight = g.childScaleHeight,
      forceExpandWidth = g.childForceExpandWidth,
      forceExpandHeight = g.childForceExpandHeight
    };

    protected override void Write(HorizontalOrVerticalLayoutGroup g, LayoutGroupValues v)
    {
      g.padding = v.padding.ToRectOffset();
      g.spacing = v.spacing;
      g.childAlignment = v.childAlignment;
      g.reverseArrangement = v.reverseArrangement;
      g.childControlWidth = v.controlChildWidth;
      g.childControlHeight = v.controlChildHeight;
      g.childScaleWidth = v.useChildScaleWidth;
      g.childScaleHeight = v.useChildScaleHeight;
      g.childForceExpandWidth = v.forceExpandWidth;
      g.childForceExpandHeight = v.forceExpandHeight;
    }

    protected override LayoutGroupValues Interpolate(LayoutGroupValues a, LayoutGroupValues b, float t)
    {
      LayoutGroupValues result = b;
      result.padding = LayoutPadding.Lerp(a.padding, b.padding, t);
      result.spacing = Mathf.LerpUnclamped(a.spacing, b.spacing, t);
      return result;
    }
  }

  [Serializable]
  public struct GridLayoutValues
  {
    public LayoutPadding padding;
    public Vector2 cellSize;
    public Vector2 spacing;
    public GridLayoutGroup.Corner startCorner;
    public GridLayoutGroup.Axis startAxis;
    public TextAnchor childAlignment;
    public GridLayoutGroup.Constraint constraint;
    [Min(1)] public int constraintCount;
  }

  [Serializable, AspectModuleMenu("Layout/Grid Layout Group")]
  public sealed class GridLayoutModule : AspectModule<GridLayoutGroup, GridLayoutValues>
  {
    public override bool SupportsInterpolation => true;

    protected override GridLayoutValues Read(GridLayoutGroup g) => new()
    {
      padding = LayoutPadding.From(g.padding),
      cellSize = g.cellSize,
      spacing = g.spacing,
      startCorner = g.startCorner,
      startAxis = g.startAxis,
      childAlignment = g.childAlignment,
      constraint = g.constraint,
      constraintCount = g.constraintCount
    };

    protected override void Write(GridLayoutGroup g, GridLayoutValues v)
    {
      g.padding = v.padding.ToRectOffset();
      g.cellSize = v.cellSize;
      g.spacing = v.spacing;
      g.startCorner = v.startCorner;
      g.startAxis = v.startAxis;
      g.childAlignment = v.childAlignment;
      g.constraint = v.constraint;
      g.constraintCount = Mathf.Max(1, v.constraintCount);
    }

    protected override GridLayoutValues Interpolate(GridLayoutValues a, GridLayoutValues b, float t)
    {
      GridLayoutValues result = b;
      result.padding = LayoutPadding.Lerp(a.padding, b.padding, t);
      result.cellSize = Vector2.LerpUnclamped(a.cellSize, b.cellSize, t);
      result.spacing = Vector2.LerpUnclamped(a.spacing, b.spacing, t);
      return result;
    }
  }

  [Serializable]
  public struct LayoutElementValues
  {
    public bool ignoreLayout;
    public float minWidth;
    public float minHeight;
    public float preferredWidth;
    public float preferredHeight;
    public float flexibleWidth;
    public float flexibleHeight;
    public int layoutPriority;
  }

  [Serializable, AspectModuleMenu("Layout/Layout Element")]
  public sealed class LayoutElementModule : AspectModule<LayoutElement, LayoutElementValues>
  {
    public override bool SupportsInterpolation => true;

    protected override LayoutElementValues Read(LayoutElement e) => new()
    {
      ignoreLayout = e.ignoreLayout,
      minWidth = e.minWidth,
      minHeight = e.minHeight,
      preferredWidth = e.preferredWidth,
      preferredHeight = e.preferredHeight,
      flexibleWidth = e.flexibleWidth,
      flexibleHeight = e.flexibleHeight,
      layoutPriority = e.layoutPriority
    };

    protected override void Write(LayoutElement e, LayoutElementValues v)
    {
      e.ignoreLayout = v.ignoreLayout;
      e.minWidth = v.minWidth;
      e.minHeight = v.minHeight;
      e.preferredWidth = v.preferredWidth;
      e.preferredHeight = v.preferredHeight;
      e.flexibleWidth = v.flexibleWidth;
      e.flexibleHeight = v.flexibleHeight;
      e.layoutPriority = v.layoutPriority;
    }

    protected override LayoutElementValues Interpolate(LayoutElementValues a, LayoutElementValues b, float t)
    {
      // -1 means "not set" in LayoutElement and must not be interpolated.
      static float Lerp(float x, float y, float k) => x < 0f || y < 0f ? y : Mathf.LerpUnclamped(x, y, k);

      LayoutElementValues result = b;
      result.minWidth = Lerp(a.minWidth, b.minWidth, t);
      result.minHeight = Lerp(a.minHeight, b.minHeight, t);
      result.preferredWidth = Lerp(a.preferredWidth, b.preferredWidth, t);
      result.preferredHeight = Lerp(a.preferredHeight, b.preferredHeight, t);
      result.flexibleWidth = Lerp(a.flexibleWidth, b.flexibleWidth, t);
      result.flexibleHeight = Lerp(a.flexibleHeight, b.flexibleHeight, t);
      return result;
    }
  }

  [Serializable]
  public struct ContentSizeFitterValues
  {
    public ContentSizeFitter.FitMode horizontalFit;
    public ContentSizeFitter.FitMode verticalFit;
  }

  [Serializable, AspectModuleMenu("Layout/Content Size Fitter")]
  public sealed class ContentSizeFitterModule : AspectModule<ContentSizeFitter, ContentSizeFitterValues>
  {
    protected override ContentSizeFitterValues Read(ContentSizeFitter f) =>
      new() { horizontalFit = f.horizontalFit, verticalFit = f.verticalFit };

    protected override void Write(ContentSizeFitter f, ContentSizeFitterValues v)
    {
      f.horizontalFit = v.horizontalFit;
      f.verticalFit = v.verticalFit;
    }
  }

  [Serializable]
  public struct AspectRatioFitterValues
  {
    public AspectRatioFitter.AspectMode aspectMode;
    public float aspectRatio;
  }

  [Serializable, AspectModuleMenu("Layout/Aspect Ratio Fitter")]
  public sealed class AspectRatioFitterModule : AspectModule<AspectRatioFitter, AspectRatioFitterValues>
  {
    public override bool SupportsInterpolation => true;

    protected override AspectRatioFitterValues Read(AspectRatioFitter f) =>
      new() { aspectMode = f.aspectMode, aspectRatio = f.aspectRatio };

    protected override void Write(AspectRatioFitter f, AspectRatioFitterValues v)
    {
      f.aspectMode = v.aspectMode;
      f.aspectRatio = Mathf.Max(0.001f, v.aspectRatio);
    }

    protected override AspectRatioFitterValues Interpolate(AspectRatioFitterValues a, AspectRatioFitterValues b, float t) =>
      new() { aspectMode = b.aspectMode, aspectRatio = Mathf.LerpUnclamped(a.aspectRatio, b.aspectRatio, t) };
  }

  [Serializable]
  public struct CanvasScalerValues
  {
    public CanvasScaler.ScaleMode scaleMode;
    public float scaleFactor;
    public Vector2 referenceResolution;
    public CanvasScaler.ScreenMatchMode screenMatchMode;
    [Range(0f, 1f)] public float matchWidthOrHeight;
    public float referencePixelsPerUnit;
  }

  [Serializable, AspectModuleMenu("Layout/Canvas Scaler")]
  public sealed class CanvasScalerModule : AspectModule<CanvasScaler, CanvasScalerValues>
  {
    public override bool SupportsInterpolation => true;

    protected override CanvasScalerValues Read(CanvasScaler s) => new()
    {
      scaleMode = s.uiScaleMode,
      scaleFactor = s.scaleFactor,
      referenceResolution = s.referenceResolution,
      screenMatchMode = s.screenMatchMode,
      matchWidthOrHeight = s.matchWidthOrHeight,
      referencePixelsPerUnit = s.referencePixelsPerUnit
    };

    protected override void Write(CanvasScaler s, CanvasScalerValues v)
    {
      s.uiScaleMode = v.scaleMode;
      s.scaleFactor = Mathf.Max(0.01f, v.scaleFactor);
      s.referenceResolution = v.referenceResolution;
      s.screenMatchMode = v.screenMatchMode;
      s.matchWidthOrHeight = v.matchWidthOrHeight;
      s.referencePixelsPerUnit = Mathf.Max(0.01f, v.referencePixelsPerUnit);
    }

    protected override CanvasScalerValues Interpolate(CanvasScalerValues a, CanvasScalerValues b, float t)
    {
      CanvasScalerValues result = b;
      result.scaleFactor = Mathf.LerpUnclamped(a.scaleFactor, b.scaleFactor, t);
      result.referenceResolution = Vector2.LerpUnclamped(a.referenceResolution, b.referenceResolution, t);
      result.matchWidthOrHeight = Mathf.LerpUnclamped(a.matchWidthOrHeight, b.matchWidthOrHeight, t);
      return result;
    }
  }
}
