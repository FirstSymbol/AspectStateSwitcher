using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace FirstSymbol.AspectRatioSwitcher.Editor
{
  [CustomEditor(typeof(AspectSwitcher))]
  internal sealed class AspectSwitcherEditor : UnityEditor.Editor
  {
    private readonly List<AspectStateDefinition> _editModeStates = new();

    private AspectDiagram _diagram;
    private Label _aspectBadge;
    private Label _stateBadge;
    private VisualElement _previewButtons;
    private VisualElement _previewActions;
    private Label _previewHint;
    private VisualElement _responsives;
    private PropertyField _cameraField;
    private PropertyField _rectField;
    private int _responsiveSignature = -1;

    private AspectSwitcher Switcher => (AspectSwitcher)target;

    public override VisualElement CreateInspectorGUI()
    {
      var root = new VisualElement();
      AspectEditorUtility.ApplyStyles(root);
      root.AddToClassList("ars-inspector");

      VisualElement card = AspectEditorUtility.Card("Aspect Switcher",
        "Resolves aspect states and applies them to Aspect Responsive components.", out VisualElement trailing);
      _aspectBadge = AspectEditorUtility.Badge(modifier: "muted");
      _stateBadge = AspectEditorUtility.Badge();
      trailing.Add(_aspectBadge);
      trailing.Add(_stateBadge);

      card.Add(new PropertyField(serializedObject.FindProperty("profile")));
      _diagram = new AspectDiagram { IsActive = IsStateActive };
      _diagram.StateClicked += state => AspectPreview.Apply(Switcher.Profile, state);
      card.Add(_diagram);
      root.Add(card);

      Foldout preview = AspectEditorUtility.Section("Preview", "SwitcherPreview");
      _previewHint = new Label();
      _previewHint.AddToClassList("ars-hint");
      preview.Add(_previewHint);
      _previewButtons = new VisualElement();
      _previewButtons.AddToClassList("ars-chips");
      preview.Add(_previewButtons);
      _previewActions = new VisualElement();
      _previewActions.AddToClassList("ars-row");
      _previewActions.Add(AspectEditorUtility.Button("Revert preview", AspectPreview.Revert, "Restore the values that existed before the preview."));
      _previewActions.Add(AspectEditorUtility.Button("Keep values", AspectPreview.Keep, "Leave the previewed values in the scene."));
      preview.Add(_previewActions);
      preview.Add(AspectEditorUtility.Button("Show Scene View overlay", AspectPreviewOverlay.ShowInSceneView,
        "Preview states from the Scene View toolbar."));
      root.Add(preview);

      Foldout measuring = AspectEditorUtility.Section("Measuring", "SwitcherMeasuring");
      SerializedProperty source = serializedObject.FindProperty("source");
      measuring.Add(new PropertyField(source));
      _cameraField = new PropertyField(serializedObject.FindProperty("targetCamera"));
      _rectField = new PropertyField(serializedObject.FindProperty("targetRect"));
      measuring.Add(_cameraField);
      measuring.Add(_rectField);
      measuring.Add(new PropertyField(serializedObject.FindProperty("stabilizationTime")));
      measuring.Add(new PropertyField(serializedObject.FindProperty("isMain")));
      measuring.TrackPropertyValue(source, UpdateSourceFields);
      UpdateSourceFields(source);
      root.Add(measuring);

      Foldout transition = AspectEditorUtility.Section("Transition", "SwitcherTransition");
      transition.Add(new PropertyField(serializedObject.FindProperty("defaultTransition"), "Default Transition"));
      root.Add(transition);

      Foldout events = AspectEditorUtility.Section("Events", "SwitcherEvents", expandedByDefault: false);
      events.Add(new PropertyField(serializedObject.FindProperty("onStateChanged")));
      root.Add(events);

      Foldout connected = AspectEditorUtility.Section("Responsive Objects", "SwitcherResponsives");
      _responsives = new VisualElement();
      connected.Add(_responsives);
      root.Add(connected);

      AspectPreview.Changed += Refresh;
      root.RegisterCallback<DetachFromPanelEvent>(_ => AspectPreview.Changed -= Refresh);
      root.TrackSerializedObjectValue(serializedObject, _ => Refresh());
      root.schedule.Execute(Tick).Every(250);
      Refresh();
      return root;
    }

    private void UpdateSourceFields(SerializedProperty source)
    {
      var value = (AspectSwitcher.AspectSource)source.enumValueIndex;
      _cameraField.style.display = value == AspectSwitcher.AspectSource.Camera ? DisplayStyle.Flex : DisplayStyle.None;
      _rectField.style.display = value == AspectSwitcher.AspectSource.RectTransform ? DisplayStyle.Flex : DisplayStyle.None;
    }

    private bool IsStateActive(AspectStateDefinition state)
    {
      if (Application.isPlaying)
        return Switcher.ActiveStates.Contains(state);
      if (AspectPreview.IsActive)
        return AspectPreview.State == state;
      return _editModeStates.Contains(state);
    }

    private void Tick()
    {
      if (target == null)
        return;

      AspectProfile profile = Switcher.Profile;
      float aspect = Application.isPlaying ? Switcher.CurrentAspect : AspectEditorUtility.GameViewAspect;
      if (profile != null && !Application.isPlaying)
        profile.GetMatchingStates(aspect, _editModeStates);

      AspectStateDefinition primary = Application.isPlaying
        ? Switcher.PrimaryState
        : AspectPreview.IsActive ? AspectPreview.State : _editModeStates.FirstOrDefault();

      _aspectBadge.text = $"{(Application.isPlaying ? "Aspect" : "Game view")} {AspectRange.Format(aspect)}";
      AspectEditorUtility.SetBadge(_stateBadge, primary != null ? primary.name : "No state", primary?.color);

      _diagram.Profile = profile;
      _diagram.Marker = aspect;
      _diagram.MarkerLabel = Application.isPlaying ? "Now" : "Game";
      _diagram.MarkDirtyRepaint();

      RefreshResponsives();
    }

    private void Refresh()
    {
      if (target == null || _previewButtons == null)
        return;

      _previewButtons.Clear();
      AspectProfile profile = Switcher.Profile;
      _diagram.Profile = profile;
      _diagram.SelectedId = AspectPreview.IsActive ? AspectPreview.State.Id : null;

      if (profile == null)
      {
        _previewHint.text = "Assign a profile to preview states.";
        _previewActions.style.display = DisplayStyle.None;
        return;
      }

      _previewHint.text = Application.isPlaying
        ? "Forces a state on the running switchers."
        : "Applies a state to every Aspect Responsive using this profile. Revert restores the original values.";

      foreach (AspectStateDefinition state in profile.States)
      {
        if (state == null)
          continue;
        AspectStateDefinition captured = state;
        Button chip = AspectEditorUtility.Button(state.name, () => AspectPreview.Apply(profile, captured),
          state.range.ToString(), "ars-chip");
        chip.style.borderLeftColor = state.color;
        chip.EnableInClassList("ars-chip--selected", AspectPreview.State == state);
        _previewButtons.Add(chip);
      }

      _previewActions.style.display = AspectPreview.IsActive ? DisplayStyle.Flex : DisplayStyle.None;
      _responsiveSignature = -1;
      RefreshResponsives();
    }

    private void RefreshResponsives()
    {
      IEnumerable<AspectResponsive> list = Application.isPlaying
        ? Switcher.Responsives
        : AspectEditorUtility.FindInLoadedScenes<AspectResponsive>()
          .Where(r => AspectEditorUtility.FindSwitcher(r) == Switcher);
      AspectResponsive[] responsives = list.Where(r => r != null).ToArray();

      int signature = responsives.Aggregate(17, (h, r) => h * 31 + RuntimeHelpers.GetHashCode(r));
      if (signature == _responsiveSignature)
        return;
      _responsiveSignature = signature;

      _responsives.Clear();
      if (responsives.Length == 0)
      {
        var empty = new Label("No Aspect Responsive components use this switcher.");
        empty.AddToClassList("ars-hint");
        _responsives.Add(empty);
        return;
      }

      foreach (AspectResponsive responsive in responsives)
      {
        var row = new VisualElement();
        row.AddToClassList("ars-list-row");
        var name = new Label(responsive.name);
        name.AddToClassList("ars-list-row__name");
        var count = AspectEditorUtility.Badge($"{responsive.Modules.Count} modules", "muted");
        AspectResponsive captured = responsive;
        row.Add(name);
        row.Add(count);
        row.Add(AspectEditorUtility.Button("Select", () => Selection.activeObject = captured.gameObject, null, "ars-button--small"));
        _responsives.Add(row);
      }
    }
  }
}
