using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace FirstSymbol.AspectRatioSwitcher.Editor
{
  [CustomEditor(typeof(AspectProfile))]
  internal sealed class AspectProfileEditor : UnityEditor.Editor
  {
    private AspectDiagram _diagram;
    private HelpBox _problems;
    private ListView _list;

    private AspectProfile Profile => (AspectProfile)target;

    public override VisualElement CreateInspectorGUI()
    {
      var root = new VisualElement();
      AspectEditorUtility.ApplyStyles(root);
      root.AddToClassList("ars-inspector");

      VisualElement card = AspectEditorUtility.Card("Aspect Profile",
        "States are matched by aspect ratio (width / height). The narrowest matching state wins; broader states are fallbacks. " +
        "Select a state and drag its edges to edit the range.",
        out VisualElement trailing);
      trailing.Add(AspectEditorUtility.Button("Reset to defaults", ResetToDefaults));
      root.Add(card);

      _diagram = new AspectDiagram { Profile = Profile, Editable = true };
      _diagram.StateClicked += state =>
      {
        int index = Profile.IndexOf(state);
        if (index >= 0)
          _list?.SetSelection(index);
      };
      card.Add(_diagram);

      _problems = new HelpBox(string.Empty, HelpBoxMessageType.Warning);
      root.Add(_problems);

      var header = new VisualElement();
      header.AddToClassList("ars-state-header");
      header.Add(new Label("Color") { style = { width = 44 } });
      header.Add(new Label("Name") { style = { flexGrow = 1 } });
      header.Add(new Label("Range  (min – max)") { style = { flexGrow = 1.4f } });
      root.Add(header);

      _list = new ListView
      {
        bindingPath = "states",
        reorderable = true,
        reorderMode = ListViewReorderMode.Animated,
        showAddRemoveFooter = true,
        showBorder = true,
        showBoundCollectionSize = false,
        showFoldoutHeader = false,
        virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight,
        selectionType = SelectionType.Single
      };
      _list.AddToClassList("ars-state-list");
      _list.selectedIndicesChanged += indices =>
      {
        int index = indices.DefaultIfEmpty(-1).First();
        _diagram.SelectedId = index >= 0 && index < Profile.States.Count ? Profile.States[index]?.Id : null;
        _diagram.MarkDirtyRepaint();
      };
      root.Add(_list);

      root.TrackSerializedObjectValue(serializedObject, _ => Refresh());
      root.schedule.Execute(() =>
      {
        _diagram.Marker = AspectEditorUtility.GameViewAspect;
        _diagram.MarkDirtyRepaint();
      }).Every(500);

      Profile.Changed += OnProfileChanged;
      root.RegisterCallback<DetachFromPanelEvent>(_ => Profile.Changed -= OnProfileChanged);
      Refresh();
      return root;
    }

    private void OnProfileChanged()
    {
      serializedObject.Update();
      Refresh();
    }

    private void Refresh()
    {
      if (Profile.EnsureUniqueIds())
      {
        EditorUtility.SetDirty(Profile);
        serializedObject.Update();
      }

      var problems = Profile.Validate();
      _problems.text = string.Join("\n", problems);
      _problems.style.display = problems.Count > 0 ? DisplayStyle.Flex : DisplayStyle.None;
      _diagram.MarkDirtyRepaint();
    }

    private void ResetToDefaults()
    {
      if (!EditorUtility.DisplayDialog("Reset Aspect Profile",
            "Replace all states with the default set? Modules referencing removed states lose their values.",
            "Reset", "Cancel"))
        return;

      Undo.RecordObject(Profile, "Reset Aspect Profile");
      Profile.ResetToDefaults();
      EditorUtility.SetDirty(Profile);
      serializedObject.Update();
      Refresh();
    }
  }
}
