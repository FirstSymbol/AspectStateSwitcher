using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace FirstSymbol.AspectRatioSwitcher.Editor
{
  /// <summary>
  /// Applies an aspect state to the open scenes for preview. In Edit Mode the original values are kept
  /// in memory so <see cref="Revert"/> restores exactly what the preview changed; in Play Mode switchers
  /// are forced to the state.
  /// </summary>
  [InitializeOnLoad]
  internal static class AspectPreview
  {
    private const string UndoName = "Preview Aspect State";
    private static readonly Dictionary<AspectModule, object> Backup = new();

    static AspectPreview()
    {
      EditorApplication.playModeStateChanged += _ => Forget();
      EditorSceneManager.sceneClosing += (_, _) => Forget();
      PrefabStage.prefabStageClosing += _ => Forget();
    }

    public static AspectStateDefinition State { get; private set; }
    public static AspectProfile Profile { get; private set; }
    public static bool IsActive => State != null;

    public static event Action Changed;

    public static void Apply(AspectProfile profile, AspectStateDefinition state)
    {
      if (profile == null || state == null)
        return;

      if (Application.isPlaying)
      {
        foreach (AspectSwitcher switcher in AspectEditorUtility.FindInLoadedScenes<AspectSwitcher>())
          if (switcher.Profile == profile && switcher.isActiveAndEnabled)
            switcher.ForceState(state.Id);
      }
      else
      {
        var chain = new List<AspectStateDefinition>();
        profile.GetFallbackChain(state, chain);

        foreach (AspectResponsive responsive in AspectEditorUtility.FindInLoadedScenes<AspectResponsive>())
        {
          if (AspectEditorUtility.FindProfile(responsive) != profile)
            continue;

          foreach (AspectModule module in responsive.Modules)
          {
            if (module?.Target == null || Backup.ContainsKey(module))
              continue;
            Backup[module] = module.ReadCurrent();
          }

          AspectEditorUtility.RecordModuleTargets(responsive, UndoName);
          responsive.ApplyStatesInstant(chain);
          foreach (AspectModule module in responsive.Modules)
            AspectEditorUtility.MarkModified(module?.Target);
        }
      }

      State = state;
      Profile = profile;
      Changed?.Invoke();
      SceneView.RepaintAll();
    }

    /// <summary>Restores the values that existed before the preview started.</summary>
    public static void Revert()
    {
      if (Application.isPlaying)
      {
        foreach (AspectSwitcher switcher in AspectEditorUtility.FindInLoadedScenes<AspectSwitcher>())
          if (switcher.ForcedState != null)
            switcher.ClearForcedState();
      }
      else
      {
        foreach (KeyValuePair<AspectModule, object> entry in Backup)
        {
          AspectEditorUtility.RecordTarget(entry.Key, "Revert Aspect Preview");
          entry.Key.WriteCurrent(entry.Value);
          AspectEditorUtility.MarkModified(entry.Key.Target);
        }
      }

      Forget();
      SceneView.RepaintAll();
    }

    /// <summary>Keeps the previewed values in the scene.</summary>
    public static void Keep() => Forget();

    private static void Forget()
    {
      Backup.Clear();
      if (State == null)
        return;

      State = null;
      Profile = null;
      Changed?.Invoke();
    }
  }
}
