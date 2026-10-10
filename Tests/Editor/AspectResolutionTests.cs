using System.Collections.Generic;
using System.Linq;
using FirstSymbol.AspectRatioSwitcher.Modules;
using NUnit.Framework;
using UnityEngine;

namespace FirstSymbol.AspectRatioSwitcher.Tests
{
  public sealed class AspectResolutionTests
  {
    private AspectProfile _profile;

    [SetUp]
    public void SetUp()
    {
      _profile = ScriptableObject.CreateInstance<AspectProfile>();
      _profile.ResetToDefaults();
    }

    [TearDown]
    public void TearDown() => Object.DestroyImmediate(_profile);

    private string[] Matching(float aspect)
    {
      var results = new List<AspectStateDefinition>();
      _profile.GetMatchingStates(aspect, results);
      return results.Select(s => s.name).ToArray();
    }

    [Test]
    public void RangeIsHalfOpen()
    {
      var range = AspectRange.Between(1f, 2f);
      Assert.IsTrue(range.Contains(1f));
      Assert.IsTrue(range.Contains(1.999f));
      Assert.IsFalse(range.Contains(2f));
      Assert.IsTrue(AspectRange.Above(1.78f).Contains(100f));
      Assert.IsTrue(AspectRange.Below(0.75f).Contains(0.1f));
    }

    [Test]
    public void RangeContainmentAndText()
    {
      Assert.IsTrue(AspectRange.Below(1f).Contains(AspectRange.Below(0.75f)));
      Assert.IsFalse(AspectRange.Below(0.75f).Contains(AspectRange.Below(1f)));
      Assert.AreEqual("0.75 – 1", AspectRange.Between(0.75f, 1f).ToString());
      Assert.AreEqual("≥ 1.78", AspectRange.Above(1.78f).ToString());
      Assert.AreEqual("any", AspectRange.Any.ToString());
    }

    [Test]
    public void MatchingStatesAreOrderedFromMostSpecific()
    {
      CollectionAssert.AreEqual(new[] { "Portrait" }, Matching(9f / 16f));
      CollectionAssert.AreEqual(new[] { "3:4", "Portrait" }, Matching(3f / 4f));
      CollectionAssert.AreEqual(new[] { "1:1", "Portrait" }, Matching(0.95f));
      CollectionAssert.AreEqual(new[] { "1:1", "Landscape" }, Matching(1f));
      CollectionAssert.AreEqual(new[] { "4:3", "Landscape" }, Matching(4f / 3f));
      CollectionAssert.AreEqual(new[] { "Landscape" }, Matching(16f / 9f));
    }

    [Test]
    public void BroaderStatesActAsFallback()
    {
      var module = new GraphicColorModule();
      module.SetValue(_profile.Find("Landscape").Id, Color.green);
      module.SetValue(_profile.Find("4:3").Id, Color.red);

      var states = new List<AspectStateDefinition>();
      _profile.GetMatchingStates(16f / 9f, states);
      Assert.AreEqual("Landscape", AspectStateResolver.Resolve(states, module.HasValue).name);

      _profile.GetMatchingStates(4f / 3f, states);
      Assert.AreEqual("4:3", AspectStateResolver.Resolve(states, module.HasValue).name);

      _profile.GetMatchingStates(1f, states);
      Assert.AreEqual("Landscape", AspectStateResolver.Resolve(states, module.HasValue).name, "1:1 has no value, Landscape is the fallback.");

      _profile.GetMatchingStates(0.5f, states);
      Assert.IsNull(AspectStateResolver.Resolve(states, module.HasValue));
    }

    [Test]
    public void ProfileOrderDoesNotHidePreciseStates()
    {
      // A broad state listed first must not win over a precise one.
      var profile = ScriptableObject.CreateInstance<AspectProfile>();
      foreach (AspectStateDefinition existing in profile.States.ToList())
        profile.RemoveState(existing);
      profile.AddState("Broad", AspectRange.Between(1f, 1.78f), Color.white);
      profile.AddState("Compact", AspectRange.Between(1f, 1.34f), Color.white);
      profile.AddState("Landscape", AspectRange.Between(1.34f, 1.78f), Color.white);

      var states = new List<AspectStateDefinition>();
      profile.GetMatchingStates(1.2f, states);
      Assert.AreEqual("Compact", states[0].name);
      profile.GetMatchingStates(1.5f, states);
      Assert.AreEqual("Landscape", states[0].name);
      Object.DestroyImmediate(profile);
    }

    [Test]
    public void FallbackChainStartsWithForcedState()
    {
      var chain = new List<AspectStateDefinition>();
      _profile.GetFallbackChain(_profile.Find("4:3"), chain);
      CollectionAssert.AreEqual(new[] { "4:3", "Landscape" }, chain.Select(s => s.name).ToArray());
    }

    [Test]
    public void FindWorksByIdAndName()
    {
      AspectStateDefinition wide = _profile.Find("landscape");
      Assert.IsNotNull(wide);
      Assert.AreSame(wide, _profile.Find(wide.Id));
      Assert.IsNull(_profile.Find("Unknown"));
    }

    [Test]
    public void DuplicatedIdsAreRegenerated()
    {
      AspectStateDefinition first = _profile.States[0];
      _profile.States[1].SetId(first.Id);

      Assert.IsTrue(_profile.EnsureUniqueIds());
      Assert.AreEqual(_profile.States.Count, _profile.States.Select(s => s.Id).Distinct().Count());
      Assert.AreEqual(first.Id, _profile.States[0].Id, "The first occurrence keeps its id.");
    }

    [Test]
    public void ModuleValuesCanBeCapturedAppliedAndCleared()
    {
      var go = new GameObject("Test", typeof(RectTransform));
      try
      {
        var module = new RectTransformModule { TypedTarget = (RectTransform)go.transform };
        string id = _profile.Find("4:3").Id;

        ((RectTransform)go.transform).anchoredPosition = new Vector2(10, 20);
        module.Capture(id);
        ((RectTransform)go.transform).anchoredPosition = Vector2.zero;

        module.ApplyInstant(id);
        Assert.AreEqual(new Vector2(10, 20), ((RectTransform)go.transform).anchoredPosition);
        Assert.IsTrue(module.Clear(id));
        Assert.IsFalse(module.HasValue(id));
      }
      finally
      {
        Object.DestroyImmediate(go);
      }
    }

    [Test]
    public void UnknownStateValuesAreRemoved()
    {
      var module = new GraphicColorModule();
      module.SetValue(_profile.Find("4:3").Id, Color.red);
      module.SetValue("deleted-state", Color.blue);

      Assert.AreEqual(1, module.RemoveUnknownStates(_profile));
      Assert.AreEqual(1, module.StateIds.Count());
    }
  }
}
