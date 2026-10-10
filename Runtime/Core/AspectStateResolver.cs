using System;
using System.Collections.Generic;

namespace FirstSymbol.AspectRatioSwitcher
{
  /// <summary>Rules that decide which state a module uses.</summary>
  public static class AspectStateResolver
  {
    /// <summary>
    /// Negative when <paramref name="a"/> is more specific than <paramref name="b"/>:
    /// a range contained in another wins, otherwise the narrower range wins, otherwise profile order.
    /// </summary>
    public static int CompareSpecificity(AspectStateDefinition a, AspectStateDefinition b,
      IReadOnlyList<AspectStateDefinition> order)
    {
      if (ReferenceEquals(a, b))
        return 0;

      bool aInsideB = b.range.Contains(a.range);
      bool bInsideA = a.range.Contains(b.range);
      if (aInsideB && !bInsideA)
        return -1;
      if (bInsideA && !aInsideB)
        return 1;

      int span = a.range.Span.CompareTo(b.range.Span);
      if (span != 0)
        return span;

      return IndexOf(order, a).CompareTo(IndexOf(order, b));
    }

    public static void SortBySpecificity(List<AspectStateDefinition> states, IReadOnlyList<AspectStateDefinition> order)
    {
      if (states.Count > 1)
        states.Sort((a, b) => CompareSpecificity(a, b, order));
    }

    /// <summary>First state of the prioritized list for which <paramref name="hasValue"/> is true.</summary>
    public static AspectStateDefinition Resolve(IReadOnlyList<AspectStateDefinition> prioritized,
      Func<string, bool> hasValue)
    {
      if (prioritized == null)
        return null;

      for (int i = 0; i < prioritized.Count; i++)
        if (hasValue(prioritized[i].Id))
          return prioritized[i];

      return null;
    }

    private static int IndexOf(IReadOnlyList<AspectStateDefinition> order, AspectStateDefinition state)
    {
      if (order == null)
        return 0;

      for (int i = 0; i < order.Count; i++)
        if (ReferenceEquals(order[i], state))
          return i;

      return int.MaxValue;
    }
  }
}
