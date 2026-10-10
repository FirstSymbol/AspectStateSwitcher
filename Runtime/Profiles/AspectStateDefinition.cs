using System;
using UnityEngine;

namespace FirstSymbol.AspectRatioSwitcher
{
  /// <summary>
  /// A named aspect state of an <see cref="AspectProfile"/>. Modules reference states by <see cref="Id"/>,
  /// so renaming or reordering states never breaks saved data.
  /// </summary>
  [Serializable]
  public sealed class AspectStateDefinition
  {
    [SerializeField, HideInInspector] private string id;
    public string name;
    public Color color = Color.gray;
    public AspectRange range = AspectRange.Any;

    public AspectStateDefinition()
    {
      id = NewId();
    }

    public AspectStateDefinition(string name, AspectRange range, Color color)
    {
      id = NewId();
      this.name = name;
      this.range = range;
      this.color = color;
    }

    public string Id => id;

    public static string NewId() => Guid.NewGuid().ToString("N");

    internal void SetId(string value) => id = value;

    public override string ToString() => $"{name} ({range})";
  }
}
