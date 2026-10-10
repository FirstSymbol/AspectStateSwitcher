using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace FirstSymbol.AspectRatioSwitcher.Editor
{
  internal static class AspectEditorUtility
  {
    private const string StyleSheetName = "AspectRatioSwitcherEditor";
    private const string EditingProfileKey = "FirstSymbol.AspectRatioSwitcher.EditingProfile";
    private static StyleSheet _styleSheet;

    public static void ApplyStyles(VisualElement element)
    {
      if (_styleSheet == null)
      {
        string path = AssetDatabase.FindAssets($"{StyleSheetName} t:StyleSheet")
          .Select(AssetDatabase.GUIDToAssetPath)
          .FirstOrDefault(p => p.EndsWith($"/{StyleSheetName}.uss", StringComparison.Ordinal));
        if (path != null)
          _styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(path);
      }

      if (_styleSheet != null && !element.styleSheets.Contains(_styleSheet))
        element.styleSheets.Add(_styleSheet);

      element.AddToClassList("ars-root");
      element.EnableInClassList("ars-root--light", !EditorGUIUtility.isProSkin);
    }

    public static Label Badge(string text = null, string modifier = null)
    {
      var badge = new Label(text);
      badge.AddToClassList("ars-badge");
      if (modifier != null)
        badge.AddToClassList($"ars-badge--{modifier}");
      return badge;
    }

    public static void SetBadge(Label badge, string text, Color? color)
    {
      badge.text = text;
      badge.style.backgroundColor = color.HasValue ? new StyleColor(color.Value) : StyleKeyword.Null;
      badge.style.color = color.HasValue ? ReadableTextColor(color.Value) : StyleKeyword.Null;
    }

    public static Color ReadableTextColor(Color background)
    {
      float luminance = 0.299f * background.r + 0.587f * background.g + 0.114f * background.b;
      return luminance > 0.6f ? new Color(0.1f, 0.1f, 0.1f) : Color.white;
    }

    public static Foldout Section(string title, string key, bool expandedByDefault = true)
    {
      string sessionKey = $"FirstSymbol.AspectRatioSwitcher.Section.{key}";
      var foldout = new Foldout { text = title, value = SessionState.GetBool(sessionKey, expandedByDefault) };
      foldout.AddToClassList("ars-section");
      foldout.RegisterValueChangedCallback(evt =>
      {
        if (evt.target == foldout)
          SessionState.SetBool(sessionKey, evt.newValue);
      });
      return foldout;
    }

    public static VisualElement Card(string title, string subtitle, out VisualElement trailing)
    {
      var card = new VisualElement();
      card.AddToClassList("ars-card");
      var row = new VisualElement();
      row.AddToClassList("ars-row");
      var titles = new VisualElement();
      titles.AddToClassList("ars-card__titles");
      var titleLabel = new Label(title);
      titleLabel.AddToClassList("ars-card__title");
      titles.Add(titleLabel);
      if (!string.IsNullOrEmpty(subtitle))
      {
        var subtitleLabel = new Label(subtitle);
        subtitleLabel.name = "subtitle";
        subtitleLabel.AddToClassList("ars-card__subtitle");
        titles.Add(subtitleLabel);
      }

      trailing = new VisualElement();
      trailing.AddToClassList("ars-row");
      row.Add(titles);
      row.Add(trailing);
      card.Add(row);
      return card;
    }

    public static Button Button(string text, Action action, string tooltip = null, params string[] classes)
    {
      var button = new Button(action) { text = text, tooltip = tooltip };
      button.AddToClassList("ars-button");
      foreach (string cls in classes)
        button.AddToClassList(cls);
      return button;
    }

    /// <summary>Aspect of the main Game view.</summary>
    public static float GameViewAspect
    {
      get
      {
        Vector2 size = Handles.GetMainGameViewSize();
        return size.y > 0f ? size.x / size.y : 1f;
      }
    }

    /// <summary>Switcher a responsive component would use in Edit Mode.</summary>
    public static AspectSwitcher FindSwitcher(AspectResponsive responsive)
    {
      if (responsive == null)
        return null;
      if (responsive.ExplicitSwitcher != null)
        return responsive.ExplicitSwitcher;

      AspectSwitcher parent = responsive.GetComponentInParent<AspectSwitcher>(true);
      if (parent != null)
        return parent;

      AspectSwitcher[] all = FindInLoadedScenes<AspectSwitcher>();
      return all.FirstOrDefault(s => s.IsMain && s.gameObject.scene == responsive.gameObject.scene)
             ?? all.FirstOrDefault(s => s.IsMain)
             ?? all.FirstOrDefault();
    }

    /// <summary>Profile used to edit a responsive component (switcher profile, else the last edited one).</summary>
    public static AspectProfile FindProfile(AspectResponsive responsive)
    {
      AspectSwitcher switcher = FindSwitcher(responsive);
      if (switcher != null && switcher.Profile != null)
        return switcher.Profile;
      return EditingProfile;
    }

    /// <summary>Profile chosen for editing when no switcher is available (e.g. in Prefab Mode).</summary>
    public static AspectProfile EditingProfile
    {
      get
      {
        string guid = EditorPrefs.GetString(EditingProfileKey, string.Empty);
        var stored = string.IsNullOrEmpty(guid)
          ? null
          : AssetDatabase.LoadAssetAtPath<AspectProfile>(AssetDatabase.GUIDToAssetPath(guid));
        if (stored != null)
          return stored;

        string first = AssetDatabase.FindAssets("t:AspectProfile").FirstOrDefault();
        return first != null ? AssetDatabase.LoadAssetAtPath<AspectProfile>(AssetDatabase.GUIDToAssetPath(first)) : null;
      }
      set
      {
        if (value != null && AssetDatabase.TryGetGUIDAndLocalFileIdentifier(value, out string guid, out long _))
          EditorPrefs.SetString(EditingProfileKey, guid);
      }
    }

    /// <summary>Components of the open scenes and the open prefab stage.</summary>
    public static T[] FindInLoadedScenes<T>() where T : Component
    {
      var results = new List<T>();
      PrefabStage stage = PrefabStageUtility.GetCurrentPrefabStage();
      if (stage != null)
        results.AddRange(stage.prefabContentsRoot.GetComponentsInChildren<T>(true));

      for (int i = 0; i < SceneManager.sceneCount; i++)
      {
        Scene scene = SceneManager.GetSceneAt(i);
        if (!scene.isLoaded)
          continue;
        foreach (GameObject root in scene.GetRootGameObjects())
          results.AddRange(root.GetComponentsInChildren<T>(true));
      }

      return results.ToArray();
    }

    public static void RecordModuleTargets(AspectResponsive responsive, string undoName)
    {
      foreach (AspectModule module in responsive.Modules)
        RecordTarget(module, undoName);
    }

    public static void RecordTarget(AspectModule module, string undoName)
    {
      if (module?.Target == null)
        return;

      switch (module)
      {
        case Modules.ParentModule or Modules.SiblingIndexModule when module.Target is Transform transform:
          Undo.RegisterFullObjectHierarchyUndo(transform.parent != null ? transform.parent.gameObject : transform.gameObject, undoName);
          break;
        default:
          Undo.RecordObject(module.Target, undoName);
          break;
      }
    }

    public static void MarkModified(Object target)
    {
      if (target == null)
        return;
      EditorUtility.SetDirty(target);
      PrefabUtility.RecordPrefabInstancePropertyModifications(target);
    }

    public static IEnumerable<Type> ModuleTypes() =>
      TypeCache.GetTypesDerivedFrom<AspectModule>()
        .Where(t => !t.IsAbstract && !t.IsGenericTypeDefinition && t.GetConstructor(Type.EmptyTypes) != null);

    public static string ModuleMenuPath(Type type)
    {
      var menu = (AspectModuleMenuAttribute)Attribute.GetCustomAttribute(type, typeof(AspectModuleMenuAttribute));
      return menu != null ? menu.Path : $"Custom/{ObjectNames.NicifyVariableName(type.Name)}";
    }
  }
}
