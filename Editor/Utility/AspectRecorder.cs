using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace FirstSymbol.AspectRatioSwitcher.Editor
{
  /// <summary>
  /// Record mode: while active, edits of module targets (inspector or scene tools) are captured
  /// into the selected state of the recording component.
  /// </summary>
  [InitializeOnLoad]
  internal static class AspectRecorder
  {
    private static readonly HashSet<AspectModule> Dirty = new();
    private static bool _scheduled;

    static AspectRecorder()
    {
      Undo.postprocessModifications += OnModifications;
      EditorApplication.playModeStateChanged += _ => Stop();
    }

    public static AspectResponsive Responsive { get; private set; }
    public static string StateId { get; private set; }

    public static bool IsRecording => Responsive != null && !string.IsNullOrEmpty(StateId);

    public static event Action Changed;
    public static event Action Captured;

    public static bool IsRecordingFor(AspectResponsive responsive) => IsRecording && Responsive == responsive;

    public static void Start(AspectResponsive responsive, string stateId)
    {
      Responsive = responsive;
      StateId = stateId;
      Changed?.Invoke();
    }

    public static void Stop()
    {
      if (Responsive == null && StateId == null)
        return;

      Responsive = null;
      StateId = null;
      Dirty.Clear();
      EditorApplication.update -= CaptureDirty;
      _scheduled = false;
      Changed?.Invoke();
    }

    private static UndoPropertyModification[] OnModifications(UndoPropertyModification[] modifications)
    {
      if (!IsRecording)
        return modifications;

      foreach (UndoPropertyModification modification in modifications)
      {
        Object modified = modification.currentValue?.target;
        if (modified == null || modified == Responsive)
          continue;

        foreach (AspectModule module in Responsive.Modules)
          if (module != null && Targets(module.Target, modified))
            Dirty.Add(module);
      }

      if (Dirty.Count > 0 && !_scheduled)
      {
        _scheduled = true;
        // Values are written after this callback, so capture on the next editor update.
        // (EditorApplication.update is used instead of delayCall, which is not pumped while the editor is unfocused.)
        EditorApplication.update += CaptureDirty;
      }

      return modifications;
    }

    private static void CaptureDirty()
    {
      EditorApplication.update -= CaptureDirty;
      _scheduled = false;
      if (!IsRecording || Dirty.Count == 0)
      {
        Dirty.Clear();
        return;
      }

      Undo.RecordObject(Responsive, "Record Aspect State");
      foreach (AspectModule module in Dirty)
        module.Capture(StateId);
      Dirty.Clear();

      AspectEditorUtility.MarkModified(Responsive);
      Captured?.Invoke();
    }

    private static bool Targets(Object target, Object modified)
    {
      if (target == null)
        return false;
      if (target == modified)
        return true;

      // Activating a GameObject or editing its transform is reported on the GameObject or Transform.
      return target switch
      {
        GameObject go => modified is Component c && c.gameObject == go && c is Transform,
        Transform t => modified == t.gameObject,
        _ => false
      };
    }
  }
}
