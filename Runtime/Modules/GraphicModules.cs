using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FirstSymbol.AspectRatioSwitcher.Modules
{
  [Serializable]
  public struct CanvasGroupValues
  {
    [Range(0f, 1f)] public float alpha;
    public bool interactable;
    public bool blocksRaycasts;
  }

  [Serializable, AspectModuleMenu("UI/Canvas Group")]
  public sealed class CanvasGroupModule : AspectModule<CanvasGroup, CanvasGroupValues>
  {
    public override bool SupportsInterpolation => true;

    protected override CanvasGroupValues Read(CanvasGroup g) =>
      new() { alpha = g.alpha, interactable = g.interactable, blocksRaycasts = g.blocksRaycasts };

    protected override void Write(CanvasGroup g, CanvasGroupValues v)
    {
      g.alpha = v.alpha;
      g.interactable = v.interactable;
      g.blocksRaycasts = v.blocksRaycasts;
    }

    protected override CanvasGroupValues Interpolate(CanvasGroupValues a, CanvasGroupValues b, float t) =>
      new() { alpha = Mathf.LerpUnclamped(a.alpha, b.alpha, t), interactable = b.interactable, blocksRaycasts = b.blocksRaycasts };
  }

  /// <summary>Color of any Graphic (Image, RawImage, Text, TextMeshPro...).</summary>
  [Serializable, AspectModuleMenu("UI/Graphic Color")]
  public sealed class GraphicColorModule : AspectModule<Graphic, Color>
  {
    public override bool SupportsInterpolation => true;
    protected override Color Read(Graphic g) => g.color;
    protected override void Write(Graphic g, Color value) => g.color = value;
    protected override Color Interpolate(Color a, Color b, float t) => Color.LerpUnclamped(a, b, t);
  }

  [Serializable]
  public struct ImageValues
  {
    public Sprite sprite;
    public Image.Type type;
    public bool preserveAspect;
    public float pixelsPerUnitMultiplier;
    [Range(0f, 1f)] public float fillAmount;
  }

  [Serializable, AspectModuleMenu("UI/Image")]
  public sealed class ImageModule : AspectModule<Image, ImageValues>
  {
    public override bool SupportsInterpolation => true;

    protected override ImageValues Read(Image i) => new()
    {
      sprite = i.sprite,
      type = i.type,
      preserveAspect = i.preserveAspect,
      pixelsPerUnitMultiplier = i.pixelsPerUnitMultiplier,
      fillAmount = i.fillAmount
    };

    protected override void Write(Image i, ImageValues v)
    {
      i.sprite = v.sprite;
      i.type = v.type;
      i.preserveAspect = v.preserveAspect;
      i.pixelsPerUnitMultiplier = Mathf.Max(0.01f, v.pixelsPerUnitMultiplier);
      i.fillAmount = v.fillAmount;
    }

    protected override ImageValues Interpolate(ImageValues a, ImageValues b, float t)
    {
      ImageValues result = b;
      result.pixelsPerUnitMultiplier = Mathf.LerpUnclamped(a.pixelsPerUnitMultiplier, b.pixelsPerUnitMultiplier, t);
      result.fillAmount = Mathf.LerpUnclamped(a.fillAmount, b.fillAmount, t);
      return result;
    }
  }

  [Serializable]
  public struct TextValues
  {
    public float fontSize;
    public bool autoSize;
    public float fontSizeMin;
    public float fontSizeMax;
    public TextAlignmentOptions alignment;
    public Vector4 margin;
  }

  /// <summary>Size, auto sizing, alignment and margins of a TextMeshPro text.</summary>
  [Serializable, AspectModuleMenu("UI/TextMeshPro Text")]
  public sealed class TextModule : AspectModule<TMP_Text, TextValues>
  {
    public override bool SupportsInterpolation => true;

    protected override TextValues Read(TMP_Text text) => new()
    {
      fontSize = text.fontSize,
      autoSize = text.enableAutoSizing,
      fontSizeMin = text.fontSizeMin,
      fontSizeMax = text.fontSizeMax,
      alignment = text.alignment,
      margin = text.margin
    };

    protected override void Write(TMP_Text text, TextValues v)
    {
      text.enableAutoSizing = v.autoSize;
      text.fontSizeMin = v.fontSizeMin;
      text.fontSizeMax = v.fontSizeMax;
      text.fontSize = v.fontSize;
      text.alignment = v.alignment;
      text.margin = v.margin;
    }

    protected override TextValues Interpolate(TextValues a, TextValues b, float t)
    {
      TextValues result = b;
      result.fontSize = Mathf.LerpUnclamped(a.fontSize, b.fontSize, t);
      result.fontSizeMin = Mathf.LerpUnclamped(a.fontSizeMin, b.fontSizeMin, t);
      result.fontSizeMax = Mathf.LerpUnclamped(a.fontSizeMax, b.fontSizeMax, t);
      result.margin = Vector4.LerpUnclamped(a.margin, b.margin, t);
      return result;
    }
  }
}
