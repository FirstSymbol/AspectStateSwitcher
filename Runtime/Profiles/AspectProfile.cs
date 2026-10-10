using System;
using System.Collections.Generic;
using UnityEngine;

namespace FirstSymbol.AspectRatioSwitcher
{
  /// <summary>
  /// The set of aspect states used by switchers. States may overlap: the most specific (narrowest)
  /// matching state wins, broader states act as fallbacks.
  /// </summary>
  [CreateAssetMenu(fileName = "AspectProfile", menuName = "Aspect Ratio Switcher/Aspect Profile", order = 0)]
  public sealed class AspectProfile : ScriptableObject
  {
    [SerializeField] private List<AspectStateDefinition> states = new();

    public IReadOnlyList<AspectStateDefinition> States => states;

    /// <summary>Raised when the profile is edited (in the editor) or reset.</summary>
    public event Action Changed;

    /// <summary>Finds a state by id, or by name (case-insensitive).</summary>
    public AspectStateDefinition Find(string idOrName)
    {
      if (string.IsNullOrEmpty(idOrName))
        return null;

      foreach (AspectStateDefinition state in states)
        if (state != null && state.Id == idOrName)
          return state;

      foreach (AspectStateDefinition state in states)
        if (state != null && string.Equals(state.name, idOrName, StringComparison.OrdinalIgnoreCase))
          return state;

      return null;
    }

    public bool TryGet(string id, out AspectStateDefinition state)
    {
      foreach (AspectStateDefinition candidate in states)
      {
        if (candidate != null && candidate.Id == id)
        {
          state = candidate;
          return true;
        }
      }

      state = null;
      return false;
    }

    public int IndexOf(AspectStateDefinition state) => states.IndexOf(state);

    /// <summary>Fills <paramref name="results"/> with states containing the aspect, most specific first.</summary>
    public void GetMatchingStates(float aspect, List<AspectStateDefinition> results)
    {
      results.Clear();
      foreach (AspectStateDefinition state in states)
        if (state != null && state.range.IsValid && state.range.Contains(aspect))
          results.Add(state);

      AspectStateResolver.SortBySpecificity(results, states);
    }

    /// <summary>
    /// Fills <paramref name="results"/> with the state followed by every broader state containing it.
    /// Used to force a state regardless of the measured aspect.
    /// </summary>
    public void GetFallbackChain(AspectStateDefinition state, List<AspectStateDefinition> results)
    {
      results.Clear();
      if (state == null)
        return;

      foreach (AspectStateDefinition candidate in states)
        if (candidate != null && candidate != state && candidate.range.Contains(state.range))
          results.Add(candidate);

      AspectStateResolver.SortBySpecificity(results, states);
      results.Insert(0, state);
    }

    public AspectStateDefinition AddState(string stateName, AspectRange range, Color color)
    {
      var state = new AspectStateDefinition(stateName, range, color);
      states.Add(state);
      NotifyChanged();
      return state;
    }

    public bool RemoveState(AspectStateDefinition state)
    {
      bool removed = states.Remove(state);
      if (removed)
        NotifyChanged();
      return removed;
    }

    public void ResetToDefaults()
    {
      states = CreateDefaultStates();
      NotifyChanged();
    }

    /// <summary>Human-readable problems of the profile (empty when valid).</summary>
    public List<string> Validate()
    {
      var problems = new List<string>();
      var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
      foreach (AspectStateDefinition state in states)
      {
        if (state == null)
          continue;
        if (string.IsNullOrWhiteSpace(state.name))
          problems.Add("A state has no name.");
        else if (!names.Add(state.name))
          problems.Add($"State name '{state.name}' is used more than once.");
        if (!state.range.IsValid)
          problems.Add($"State '{state.name}' has an empty range ({state.range}).");
      }

      return problems;
    }

    /// <summary>States similar to common device classes.</summary>
    public static List<AspectStateDefinition> CreateDefaultStates() => new()
    {
      new AspectStateDefinition("Portrait", AspectRange.Below(0.75f), new Color(0.36f, 0.55f, 0.95f)),
      new AspectStateDefinition("Tall", AspectRange.Between(0.75f, 1f), new Color(0.25f, 0.75f, 0.85f)),
      new AspectStateDefinition("Compact", AspectRange.Between(1f, 1.34f), new Color(0.3f, 0.78f, 0.5f)),
      new AspectStateDefinition("Landscape", AspectRange.Between(1.34f, 1.78f), new Color(0.95f, 0.7f, 0.25f)),
      new AspectStateDefinition("Wide", AspectRange.Above(1.78f), new Color(0.92f, 0.42f, 0.38f)),
      new AspectStateDefinition("Any Portrait", AspectRange.Below(1f), new Color(0.55f, 0.48f, 0.9f)),
      new AspectStateDefinition("Tablet", AspectRange.Between(0.75f, 1.34f), new Color(0.5f, 0.82f, 0.72f)),
      new AspectStateDefinition("Any Landscape", AspectRange.Above(1f), new Color(0.9f, 0.55f, 0.7f))
    };

    private void Reset() => states = CreateDefaultStates();

    private void OnValidate()
    {
      EnsureUniqueIds();
      Changed?.Invoke();
    }

    internal void NotifyChanged() => Changed?.Invoke();

    /// <summary>Gives new ids to missing or duplicated ids (e.g. after duplicating a list element).</summary>
    internal bool EnsureUniqueIds()
    {
      bool changed = false;
      var ids = new HashSet<string>();
      for (int i = 0; i < states.Count; i++)
      {
        if (states[i] == null)
        {
          states[i] = new AspectStateDefinition("State", AspectRange.Any, Color.gray);
          changed = true;
        }

        if (string.IsNullOrEmpty(states[i].Id) || !ids.Add(states[i].Id))
        {
          states[i].SetId(AspectStateDefinition.NewId());
          ids.Add(states[i].Id);
          changed = true;
        }
      }

      return changed;
    }
  }
}
