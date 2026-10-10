using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace FirstSymbol.AspectRatioSwitcher.Editor
{
  [CustomEditor(typeof(AspectResponsive))]
  internal sealed class AspectResponsiveEditor : UnityEditor.Editor
  {
    private static readonly HashSet<string> ModuleBaseFields = new() { "enabled", "target", "values" };

    private AspectProfile _profile;
    private string _stateId;
    private string _signature;

    private Label _switcherLabel;
    private Label _appliedBadge;
    private VisualElement _profileWarning;
    private VisualElement _chips;
    private Button _recordButton;
    private VisualElement _modules;
    private HelpBox _unknownStates;

    private AspectResponsive Responsive => (AspectResponsive)target;
    private string _stateKey;
    private string StateKey => _stateKey ??= $"FirstSymbol.AspectRatioSwitcher.State.{GlobalObjectId.GetGlobalObjectIdSlow(target)}";

    public override VisualElement CreateInspectorGUI()
    {
      var root = new VisualElement();
      AspectEditorUtility.ApplyStyles(root);
      root.AddToClassList("ars-inspector");

      VisualElement card = AspectEditorUtility.Card("Aspect Responsive", null, out VisualElement trailing);
      _switcherLabel = new Label();
      _switcherLabel.AddToClassList("ars-card__subtitle");
      card.Q(className: "ars-card__titles").Add(_switcherLabel);
      _appliedBadge = AspectEditorUtility.Badge();
      trailing.Add(_appliedBadge);

      card.Add(new PropertyField(serializedObject.FindProperty("switcher"), "Switcher (optional)"));

      _profileWarning = new VisualElement();
      card.Add(_profileWarning);

      var statesLabel = new Label("Edit state");
      statesLabel.AddToClassList("ars-label");
      card.Add(statesLabel);
      _chips = new VisualElement();
      _chips.AddToClassList("ars-chips");
      card.Add(_chips);

      var toolbar = new VisualElement();
      toolbar.AddToClassList("ars-row");
      toolbar.AddToClassList("ars-toolbar");
      toolbar.Add(AspectEditorUtility.Button("Capture all", CaptureAll, "Store the current values of every module in the selected state."));
      toolbar.Add(AspectEditorUtility.Button("Apply", ApplySelected, "Apply the selected state to this object."));
      toolbar.Add(AspectEditorUtility.Button("Preview scene", PreviewScene, "Apply the selected state to every object using this profile (revertible)."));
      toolbar.Add(AspectEditorUtility.Button("Clear", ClearSelected, "Remove the values of the selected state."));
      _recordButton = AspectEditorUtility.Button("● REC", ToggleRecord,
        "Record mode: edits of module targets are captured into the selected state automatically.", "ars-record");
      toolbar.Add(_recordButton);
      card.Add(toolbar);
      root.Add(card);

      Foldout modules = AspectEditorUtility.Section("Modules", "ResponsiveModules");
      _modules = new VisualElement();
      modules.Add(_modules);
      var add = AspectEditorUtility.Button("+ Add Module", ShowAddMenu, null, "ars-add");
      modules.Add(add);
      _unknownStates = new HelpBox(string.Empty, HelpBoxMessageType.Info);
      _unknownStates.Add(AspectEditorUtility.Button("Remove", RemoveUnknownStates));
      modules.Add(_unknownStates);
      root.Add(modules);

      Foldout transition = AspectEditorUtility.Section("Transition", "ResponsiveTransition");
      SerializedProperty mode = serializedObject.FindProperty("transition");
      transition.Add(new PropertyField(mode, "Transition"));
      var custom = new PropertyField(serializedObject.FindProperty("customTransition"), "Custom");
      transition.Add(custom);
      void UpdateCustom(SerializedProperty p) =>
        custom.style.display = p.enumValueIndex == (int)TransitionOverride.Custom ? DisplayStyle.Flex : DisplayStyle.None;
      UpdateCustom(mode);
      transition.TrackPropertyValue(mode, UpdateCustom);
      root.Add(transition);

      _stateId = SessionState.GetString(StateKey, null);
      AspectRecorder.Changed += RefreshHeader;
      AspectRecorder.Captured += OnRecorded;
      Undo.undoRedoPerformed += OnUndoRedo;
      root.RegisterCallback<DetachFromPanelEvent>(_ =>
      {
        AspectRecorder.Changed -= RefreshHeader;
        AspectRecorder.Captured -= OnRecorded;
        Undo.undoRedoPerformed -= OnUndoRedo;
      });

      root.TrackSerializedObjectValue(serializedObject, _ => Rebuild(force: false));
      root.schedule.Execute(RefreshHeader).Every(300);
      Rebuild(force: true);
      return root;
    }

    #region State selection

    private AspectStateDefinition SelectedState
    {
      get
      {
        if (_profile == null)
          return null;
        if (_stateId != null && _profile.TryGet(_stateId, out AspectStateDefinition state))
          return state;
        // Default to the first state that already has values.
        return _profile.States.FirstOrDefault(s => s != null && Responsive.HasAnyValue(s.Id)) ??
               _profile.States.FirstOrDefault(s => s != null);
      }
    }

    private void Select(AspectStateDefinition state)
    {
      _stateId = state?.Id;
      SessionState.SetString(StateKey, _stateId ?? string.Empty);
      if (AspectRecorder.IsRecordingFor(Responsive) && state != null)
        AspectRecorder.Start(Responsive, state.Id);
      Rebuild(force: true);
    }

    #endregion

    #region Build

    private void Rebuild(bool force)
    {
      if (target == null || _modules == null)
        return;

      serializedObject.Update();
      _profile = AspectEditorUtility.FindProfile(Responsive);
      string signature = ComputeSignature();
      if (!force && signature == _signature)
      {
        RefreshHeader();
        return;
      }

      _signature = signature;
      BuildProfileWarning();
      BuildChips();
      BuildModules();
      RefreshHeader();
    }

    private string ComputeSignature()
    {
      var parts = new List<string> { _profile != null ? RuntimeHelpers.GetHashCode(_profile).ToString() : "-", _stateId ?? "-" };
      if (_profile != null)
        parts.AddRange(_profile.States.Where(s => s != null).Select(s => s.Id + s.name + s.color));
      foreach (AspectModule module in Responsive.Modules)
        parts.Add(module == null
          ? "null"
          : $"{module.GetType().Name}:{module.Enabled}:{(module.Target != null ? RuntimeHelpers.GetHashCode(module.Target) : 0)}:{string.Join(",", module.StateIds)}");
      return string.Join("|", parts);
    }

    private void BuildProfileWarning()
    {
      _profileWarning.Clear();
      AspectSwitcher switcher = AspectEditorUtility.FindSwitcher(Responsive);
      if (switcher != null && switcher.Profile != null)
        return;

      var box = new HelpBox(switcher == null
        ? "No Aspect Switcher found. Values are edited against the profile below (useful in Prefab Mode)."
        : "The switcher has no profile. Values are edited against the profile below.", HelpBoxMessageType.Info);
      _profileWarning.Add(box);

      var field = new ObjectField("Editing profile") { objectType = typeof(AspectProfile), value = _profile };
      field.RegisterValueChangedCallback(evt =>
      {
        AspectEditorUtility.EditingProfile = evt.newValue as AspectProfile;
        Rebuild(force: true);
      });
      _profileWarning.Add(field);
    }

    private void BuildChips()
    {
      _chips.Clear();
      if (_profile == null)
      {
        var hint = new Label("Create an Aspect Profile (Create > Aspect Ratio Switcher > Aspect Profile).");
        hint.AddToClassList("ars-hint");
        _chips.Add(hint);
        return;
      }

      AspectStateDefinition selected = SelectedState;
      foreach (AspectStateDefinition state in _profile.States)
      {
        if (state == null)
          continue;

        AspectStateDefinition captured = state;
        bool hasValue = Responsive.HasAnyValue(state.Id);
        Button chip = AspectEditorUtility.Button(hasValue ? $"● {state.name}" : state.name, () => Select(captured),
          $"{state.range}{(hasValue ? " — has values" : string.Empty)}", "ars-chip");
        chip.style.borderLeftColor = state.color;
        chip.EnableInClassList("ars-chip--selected", state == selected);
        chip.EnableInClassList("ars-chip--empty", !hasValue);
        if (state == selected)
        {
          chip.style.backgroundColor = state.color;
          chip.style.color = AspectEditorUtility.ReadableTextColor(state.color);
        }

        _chips.Add(chip);
      }
    }

    private void BuildModules()
    {
      _modules.Clear();
      SerializedProperty modulesProperty = serializedObject.FindProperty("modules");
      AspectStateDefinition state = SelectedState;

      if (Responsive.Modules.Count == 0)
      {
        var empty = new Label("No modules yet. Add one to start adapting this object.");
        empty.AddToClassList("ars-hint");
        _modules.Add(empty);
      }

      for (int i = 0; i < Responsive.Modules.Count; i++)
        _modules.Add(BuildModuleCard(Responsive.Modules[i], modulesProperty.GetArrayElementAtIndex(i), i, state));

      int unknown = _profile == null
        ? 0
        : Responsive.Modules.Where(m => m != null).SelectMany(m => m.StateIds).Count(id => !_profile.TryGet(id, out _));
      _unknownStates.text = $"{unknown} value(s) belong to states that are not in '{(_profile != null ? _profile.name : "")}'.";
      _unknownStates.style.display = unknown > 0 ? DisplayStyle.Flex : DisplayStyle.None;
    }

    private VisualElement BuildModuleCard(AspectModule module, SerializedProperty property, int index, AspectStateDefinition state)
    {
      var card = new VisualElement();
      card.AddToClassList("ars-module");

      if (module == null)
      {
        card.Add(new HelpBox("Missing module type (the class was renamed or removed).", HelpBoxMessageType.Warning));
        card.Add(AspectEditorUtility.Button("Remove", () => RemoveModuleAt(index)));
        return card;
      }

      var header = new VisualElement();
      header.AddToClassList("ars-module__header");

      var enabled = new Toggle { tooltip = "Enabled" };
      enabled.BindProperty(property.FindPropertyRelative("enabled"));
      header.Add(enabled);

      var foldout = new Foldout
      {
        text = module.DisplayName,
        value = SessionState.GetBool($"{StateKey}.module.{index}", true)
      };
      foldout.AddToClassList("ars-module__foldout");
      foldout.RegisterValueChangedCallback(evt =>
      {
        if (evt.target == foldout)
          SessionState.SetBool($"{StateKey}.module.{index}", evt.newValue);
      });

      bool hasValue = state != null && module.HasValue(state.Id);
      Label status = AspectEditorUtility.Badge(hasValue ? "set" : "—", hasValue ? null : "muted");
      if (hasValue)
        AspectEditorUtility.SetBadge(status, "set", state.color);
      status.tooltip = hasValue ? $"Has a value for '{state.name}'" : "No value for the selected state";

      Toggle title = foldout.Q<Toggle>();
      title.AddToClassList("ars-module__title");
      header.Add(title);
      header.Add(status);
      header.Add(AspectEditorUtility.Button("Capture", () => CaptureModule(module), "Store the current target values in the selected state.", "ars-button--small"));
      header.Add(AspectEditorUtility.Button("Apply", () => ApplyModule(module), "Apply the selected state's value to the target.", "ars-button--small"));
      header.Add(AspectEditorUtility.Button("⋮", () => ShowModuleMenu(module, index), "More", "ars-button--small"));
      card.Add(header);

      VisualElement body = foldout.contentContainer;
      var targetField = new ObjectField("Target")
      {
        objectType = module.TargetType,
        allowSceneObjects = true
      };
      targetField.BindProperty(property.FindPropertyRelative("target"));
      body.Add(targetField);

      foreach (SerializedProperty child in Children(property))
        if (!ModuleBaseFields.Contains(child.name))
          body.Add(new PropertyField(child));

      body.Add(BuildValueField(module, property, state));

      string warning = module.GetWarning(Responsive);
      if (warning != null)
        body.Add(new HelpBox(warning, HelpBoxMessageType.Warning));

      card.Add(foldout);
      return card;
    }

    private VisualElement BuildValueField(AspectModule module, SerializedProperty property, AspectStateDefinition state)
    {
      var container = new VisualElement();
      container.AddToClassList("ars-module__value");
      if (state == null)
        return container;

      SerializedProperty values = property.FindPropertyRelative("values");
      for (int i = 0; i < values.arraySize; i++)
      {
        SerializedProperty entry = values.GetArrayElementAtIndex(i);
        if (entry.FindPropertyRelative("stateId").stringValue != state.Id)
          continue;

        SerializedProperty value = entry.FindPropertyRelative("value");
        var title = new Label($"Value · {state.name}");
        title.AddToClassList("ars-module__value-title");
        title.style.borderLeftColor = state.color;
        container.Add(title);

        if (value.hasVisibleChildren && value.propertyType == SerializedPropertyType.Generic && !IsUnityEvent(value))
        {
          foreach (SerializedProperty child in Children(value))
            container.Add(new PropertyField(child));
        }
        else
        {
          container.Add(new PropertyField(value, "Value"));
        }

        return container;
      }

      var hint = new Label($"No value for '{state.name}'. Press Capture to store the current values.");
      hint.AddToClassList("ars-hint");
      container.Add(hint);
      return container;
    }

    private static bool IsUnityEvent(SerializedProperty property) =>
      property.FindPropertyRelative("m_PersistentCalls") != null;

    private static IEnumerable<SerializedProperty> Children(SerializedProperty property)
    {
      SerializedProperty iterator = property.Copy();
      SerializedProperty end = iterator.GetEndProperty();
      if (!iterator.NextVisible(true))
        yield break;

      do
      {
        if (SerializedProperty.EqualContents(iterator, end))
          yield break;
        yield return iterator.Copy();
      } while (iterator.NextVisible(false));
    }

    private void RefreshHeader()
    {
      if (target == null || _switcherLabel == null)
        return;

      AspectSwitcher switcher = Application.isPlaying ? Responsive.ConnectedSwitcher : AspectEditorUtility.FindSwitcher(Responsive);
      string source = Responsive.ExplicitSwitcher != null
        ? "assigned"
        : switcher != null && Responsive.GetComponentInParent<AspectSwitcher>(true) == switcher ? "parent" : "main";
      _switcherLabel.text = switcher != null
        ? $"Switcher: {switcher.name} ({source})  ·  Profile: {(_profile != null ? _profile.name : "none")}"
        : $"No switcher  ·  Profile: {(_profile != null ? _profile.name : "none")}";

      if (Application.isPlaying && switcher != null && switcher.PrimaryState != null)
        AspectEditorUtility.SetBadge(_appliedBadge, $"Live: {switcher.PrimaryState.name}", switcher.PrimaryState.color);
      else if (AspectPreview.IsActive && AspectPreview.Profile == _profile)
        AspectEditorUtility.SetBadge(_appliedBadge, $"Preview: {AspectPreview.State.name}", AspectPreview.State.color);
      else
        AspectEditorUtility.SetBadge(_appliedBadge, $"{Responsive.Modules.Count} modules", null);

      bool recording = AspectRecorder.IsRecordingFor(Responsive);
      _recordButton.EnableInClassList("ars-record--on", recording);
      _recordButton.text = recording ? "■ STOP" : "● REC";
    }

    #endregion

    #region Actions

    private void CaptureAll()
    {
      AspectStateDefinition state = SelectedState;
      if (state == null)
        return;
      Modify("Capture Aspect State", () => Responsive.CaptureAll(state.Id));
    }

    private void ClearSelected()
    {
      AspectStateDefinition state = SelectedState;
      if (state == null)
        return;
      Modify("Clear Aspect State", () => Responsive.ClearState(state.Id));
    }

    private void ApplySelected()
    {
      AspectStateDefinition state = SelectedState;
      if (state == null || _profile == null)
        return;

      var chain = new List<AspectStateDefinition>();
      _profile.GetFallbackChain(state, chain);
      AspectEditorUtility.RecordModuleTargets(Responsive, "Apply Aspect State");
      Responsive.ApplyStatesInstant(chain);
      foreach (AspectModule module in Responsive.Modules)
        AspectEditorUtility.MarkModified(module?.Target);
    }

    private void PreviewScene()
    {
      AspectStateDefinition state = SelectedState;
      if (state != null)
        AspectPreview.Apply(_profile, state);
    }

    private void ToggleRecord()
    {
      if (AspectRecorder.IsRecordingFor(Responsive))
      {
        AspectRecorder.Stop();
        return;
      }

      AspectStateDefinition state = SelectedState;
      if (state != null)
        AspectRecorder.Start(Responsive, state.Id);
    }

    private void CaptureModule(AspectModule module)
    {
      AspectStateDefinition state = SelectedState;
      if (state != null)
        Modify("Capture Aspect Module", () => module.Capture(state.Id));
    }

    private void ApplyModule(AspectModule module)
    {
      AspectStateDefinition state = SelectedState;
      if (state == null)
        return;
      AspectEditorUtility.RecordTarget(module, "Apply Aspect Module");
      module.ApplyInstant(state.Id);
      AspectEditorUtility.MarkModified(module.Target);
    }

    private void ShowModuleMenu(AspectModule module, int index)
    {
      var menu = new GenericMenu();
      AspectStateDefinition state = SelectedState;
      if (state != null)
        menu.AddItem(new GUIContent($"Clear '{state.name}'"), false, () => Modify("Clear Aspect Module", () => module.Clear(state.Id)));
      menu.AddItem(new GUIContent("Clear all states"), false, () => Modify("Clear Aspect Module", () =>
      {
        foreach (string id in module.StateIds.ToArray())
          module.Clear(id);
      }));
      menu.AddSeparator(string.Empty);
      if (index > 0)
        menu.AddItem(new GUIContent("Move up"), false, () => MoveModule(index, index - 1));
      if (index < Responsive.Modules.Count - 1)
        menu.AddItem(new GUIContent("Move down"), false, () => MoveModule(index, index + 1));
      menu.AddSeparator(string.Empty);
      menu.AddItem(new GUIContent("Remove"), false, () => RemoveModuleAt(index));
      menu.ShowAsContext();
    }

    private void ShowAddMenu()
    {
      var menu = new GenericMenu();
      List<Type> types = AspectEditorUtility.ModuleTypes().OrderBy(AspectEditorUtility.ModuleMenuPath).ToList();

      // Suggest modules whose target component exists on this object.
      foreach (Type type in types)
      {
        Type targetType = ((AspectModule)Activator.CreateInstance(type)).TargetType;
        bool present = typeof(Component).IsAssignableFrom(targetType) && targetType != typeof(Component) &&
                       targetType != typeof(Transform) && Responsive.GetComponent(targetType) != null;
        if (!present)
          continue;
        Type captured = type;
        menu.AddItem(new GUIContent($"Suggested/{LastSegment(AspectEditorUtility.ModuleMenuPath(type))}"), false, () => AddModule(captured));
      }

      menu.AddSeparator(string.Empty);
      foreach (Type type in types)
      {
        Type captured = type;
        menu.AddItem(new GUIContent(AspectEditorUtility.ModuleMenuPath(type)), false, () => AddModule(captured));
      }

      menu.ShowAsContext();
    }

    private static string LastSegment(string path)
    {
      int slash = path.LastIndexOf('/');
      return slash >= 0 ? path.Substring(slash + 1) : path;
    }

    private void AddModule(Type type)
    {
      AspectStateDefinition state = SelectedState;
      Modify("Add Aspect Module", () =>
      {
        AspectModule module = Responsive.AddModule(type);
        // Start with the current values in the selected state so the module is immediately useful.
        if (state != null && module.Target != null)
          module.Capture(state.Id);
      });
    }

    private void RemoveModuleAt(int index)
    {
      Modify("Remove Aspect Module", () =>
      {
        SerializedProperty modules = serializedObject.FindProperty("modules");
        modules.DeleteArrayElementAtIndex(index);
        serializedObject.ApplyModifiedPropertiesWithoutUndo();
      });
    }

    private void MoveModule(int from, int to)
    {
      Modify("Move Aspect Module", () =>
      {
        serializedObject.FindProperty("modules").MoveArrayElement(from, to);
        serializedObject.ApplyModifiedPropertiesWithoutUndo();
      });
    }

    private void RemoveUnknownStates()
    {
      Modify("Remove Unknown Aspect States", () =>
      {
        foreach (AspectModule module in Responsive.Modules)
          module?.RemoveUnknownStates(_profile);
      });
    }

    private void Modify(string undoName, Action action)
    {
      serializedObject.ApplyModifiedProperties();
      Undo.RecordObject(Responsive, undoName);
      action();
      AspectEditorUtility.MarkModified(Responsive);
      Rebuild(force: true);
    }

    private void OnRecorded()
    {
      if (AspectRecorder.IsRecordingFor(Responsive))
        Rebuild(force: true);
    }

    private void OnUndoRedo() => Rebuild(force: true);

    #endregion
  }
}
