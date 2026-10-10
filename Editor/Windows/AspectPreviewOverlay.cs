using System.Linq;
using UnityEditor;
using UnityEditor.Overlays;
using UnityEngine;
using UnityEngine.UIElements;

namespace FirstSymbol.AspectRatioSwitcher.Editor
{
  /// <summary>Scene View panel to preview aspect states of the main switcher's profile.</summary>
  [Overlay(typeof(SceneView), Id, "Aspect Preview")]
  [Icon("d_UnityEditor.GameView")]
  internal sealed class AspectPreviewOverlay : Overlay
  {
    public const string Id = "first-symbol-aspect-preview";

    private VisualElement _root;

    public static void ShowInSceneView()
    {
      foreach (SceneView view in SceneView.sceneViews)
      {
        if (view is SceneView sceneView && sceneView.TryGetOverlay(Id, out Overlay overlay))
        {
          overlay.displayed = true;
          overlay.collapsed = false;
        }
      }
    }

    public override VisualElement CreatePanelContent()
    {
      _root = new VisualElement();
      AspectEditorUtility.ApplyStyles(_root);
      _root.AddToClassList("ars-overlay");

      AspectPreview.Changed += Build;
      EditorApplication.hierarchyChanged += Build;
      _root.RegisterCallback<DetachFromPanelEvent>(_ =>
      {
        AspectPreview.Changed -= Build;
        EditorApplication.hierarchyChanged -= Build;
      });
      _root.schedule.Execute(UpdateAspectLabel).Every(500);

      Build();
      return _root;
    }

    private static AspectProfile FindProfile()
    {
      AspectSwitcher[] switchers = AspectEditorUtility.FindInLoadedScenes<AspectSwitcher>();
      AspectSwitcher main = switchers.FirstOrDefault(s => s.IsMain && s.Profile != null) ??
                            switchers.FirstOrDefault(s => s.Profile != null);
      return main != null ? main.Profile : AspectEditorUtility.EditingProfile;
    }

    private void Build()
    {
      if (_root == null)
        return;

      _root.Clear();
      AspectProfile profile = FindProfile();
      if (profile == null)
      {
        _root.Add(new Label("No Aspect Profile found."));
        return;
      }

      var aspect = new Label { name = "aspect" };
      aspect.AddToClassList("ars-hint");
      _root.Add(aspect);

      var chips = new VisualElement();
      chips.AddToClassList("ars-chips");
      chips.AddToClassList("ars-overlay__chips");
      foreach (AspectStateDefinition state in profile.States)
      {
        if (state == null)
          continue;
        AspectStateDefinition captured = state;
        Button chip = AspectEditorUtility.Button(state.name, () => AspectPreview.Apply(profile, captured), state.range.ToString(), "ars-chip");
        chip.style.borderLeftColor = state.color;
        chip.EnableInClassList("ars-chip--selected", AspectPreview.State == state);
        chips.Add(chip);
      }

      _root.Add(chips);

      if (AspectPreview.IsActive)
      {
        var actions = new VisualElement();
        actions.AddToClassList("ars-row");
        actions.Add(AspectEditorUtility.Button("Revert", AspectPreview.Revert));
        actions.Add(AspectEditorUtility.Button("Keep", AspectPreview.Keep));
        _root.Add(actions);
      }

      UpdateAspectLabel();
    }

    private void UpdateAspectLabel()
    {
      if (_root?.Q<Label>("aspect") is not { } label)
        return;

      float aspect = AspectEditorUtility.GameViewAspect;
      AspectProfile profile = FindProfile();
      string state = string.Empty;
      if (profile != null)
      {
        var matching = new System.Collections.Generic.List<AspectStateDefinition>();
        profile.GetMatchingStates(aspect, matching);
        if (matching.Count > 0)
          state = $" → {matching[0].name}";
      }

      label.text = AspectPreview.IsActive
        ? $"Previewing {AspectPreview.State.name}"
        : $"Game view {AspectRange.Format(aspect)}{state}";
    }
  }
}
