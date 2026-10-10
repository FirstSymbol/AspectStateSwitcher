using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace FirstSymbol.AspectRatioSwitcher.Editor
{
  /// <summary>Matrix of Aspect Responsive objects × states of a profile.</summary>
  internal sealed class AspectOverviewWindow : EditorWindow
  {
    private readonly List<AspectResponsive> _rows = new();
    private ObjectField _profileField;
    private VisualElement _tableHost;
    private MultiColumnListView _table;
    private AspectProfile _profile;
    private string _search = string.Empty;

    [MenuItem("Window/Aspect Ratio Switcher/Overview", priority = 2010)]
    public static void Open()
    {
      var window = GetWindow<AspectOverviewWindow>();
      window.titleContent = new GUIContent("Aspect Overview", EditorGUIUtility.IconContent("d_UnityEditor.GameView").image);
      window.minSize = new Vector2(480, 240);
    }

    [MenuItem("Window/Aspect Ratio Switcher/Scene View Preview Overlay", priority = 2011)]
    public static void ShowOverlay() => AspectPreviewOverlay.ShowInSceneView();

    private void OnEnable() => EditorApplication.hierarchyChanged += Rebuild;

    private void OnDisable() => EditorApplication.hierarchyChanged -= Rebuild;

    public void CreateGUI()
    {
      AspectEditorUtility.ApplyStyles(rootVisualElement);
      rootVisualElement.AddToClassList("ars-overview");

      var toolbar = new Toolbar();
      _profileField = new ObjectField { objectType = typeof(AspectProfile), allowSceneObjects = false };
      _profileField.AddToClassList("ars-overview__profile");
      _profileField.RegisterValueChangedCallback(evt =>
      {
        _profile = evt.newValue as AspectProfile;
        Rebuild();
      });
      toolbar.Add(_profileField);

      var search = new ToolbarSearchField();
      search.RegisterValueChangedCallback(evt =>
      {
        _search = evt.newValue ?? string.Empty;
        Rebuild();
      });
      toolbar.Add(search);
      toolbar.Add(new ToolbarSpacer { flex = true });
      toolbar.Add(new ToolbarButton(Rebuild) { text = "Refresh" });
      rootVisualElement.Add(toolbar);

      var legend = new Label("● values for the state   ○ falls back to a broader state   · no value");
      legend.AddToClassList("ars-hint");
      legend.style.marginLeft = 6;
      rootVisualElement.Add(legend);

      _tableHost = new VisualElement { style = { flexGrow = 1 } };
      rootVisualElement.Add(_tableHost);

      AspectSwitcher main = AspectEditorUtility.FindInLoadedScenes<AspectSwitcher>().FirstOrDefault(s => s.IsMain && s.Profile != null);
      _profile = main != null ? main.Profile : AspectEditorUtility.EditingProfile;
      _profileField.SetValueWithoutNotify(_profile);
      Rebuild();
    }

    private void Rebuild()
    {
      if (_tableHost == null)
        return;

      _rows.Clear();
      _rows.AddRange(AspectEditorUtility.FindInLoadedScenes<AspectResponsive>()
        .Where(r => _profile == null || AspectEditorUtility.FindProfile(r) == _profile)
        .Where(r => string.IsNullOrEmpty(_search) || r.name.IndexOf(_search, System.StringComparison.OrdinalIgnoreCase) >= 0)
        .OrderBy(r => r.name));

      _tableHost.Clear();
      if (_profile == null)
      {
        _tableHost.Add(new HelpBox("Select an Aspect Profile.", HelpBoxMessageType.Info));
        return;
      }

      _table = new MultiColumnListView
      {
        itemsSource = _rows,
        fixedItemHeight = 22,
        showAlternatingRowBackgrounds = AlternatingRowBackground.ContentOnly,
        selectionType = SelectionType.Single,
        style = { flexGrow = 1 }
      };

      _table.columns.Add(new Column
      {
        title = "Object", width = 180, minWidth = 100, stretchable = true,
        makeCell = () => new Label { style = { unityTextAlign = TextAnchor.MiddleLeft, paddingLeft = 4 } },
        bindCell = (e, i) => ((Label)e).text = _rows[i] != null ? _rows[i].name : "(missing)"
      });
      _table.columns.Add(new Column
      {
        title = "Modules", width = 64, resizable = false,
        makeCell = () => new Label { style = { unityTextAlign = TextAnchor.MiddleCenter } },
        bindCell = (e, i) => ((Label)e).text = _rows[i] != null ? _rows[i].Modules.Count.ToString() : string.Empty
      });

      foreach (AspectStateDefinition state in _profile.States.Where(s => s != null))
      {
        AspectStateDefinition captured = state;
        var column = new Column
        {
          title = state.name, width = 74, minWidth = 50,
          makeCell = () => new Label { style = { unityTextAlign = TextAnchor.MiddleCenter, unityFontStyleAndWeight = FontStyle.Bold } },
          bindCell = (e, i) => BindCell((Label)e, _rows[i], captured)
        };
        _table.columns.Add(column);
      }

      _table.selectionChanged += selection =>
      {
        if (selection.FirstOrDefault() is AspectResponsive responsive && responsive != null)
        {
          Selection.activeObject = responsive.gameObject;
          EditorGUIUtility.PingObject(responsive.gameObject);
        }
      };

      _tableHost.Add(_table);
      if (_rows.Count == 0)
        _tableHost.Add(new HelpBox("No Aspect Responsive components use this profile in the open scenes.", HelpBoxMessageType.Info));
    }

    private void BindCell(Label cell, AspectResponsive responsive, AspectStateDefinition state)
    {
      if (responsive == null)
      {
        cell.text = string.Empty;
        return;
      }

      if (responsive.HasAnyValue(state.Id))
      {
        cell.text = "●";
        cell.style.color = state.color;
        cell.tooltip = $"{responsive.name} has values for {state.name}";
        return;
      }

      var chain = new List<AspectStateDefinition>();
      _profile.GetFallbackChain(state, chain);
      bool fallback = chain.Skip(1).Any(s => responsive.HasAnyValue(s.Id));
      cell.text = fallback ? "○" : "·";
      cell.style.color = new Color(0.6f, 0.6f, 0.6f);
      cell.tooltip = fallback ? $"Uses a broader state in {state.name}" : $"No values in {state.name}";
    }
  }
}
