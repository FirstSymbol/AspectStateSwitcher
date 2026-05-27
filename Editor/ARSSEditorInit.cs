using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

namespace AspectSwitcher
{
    // Unity 6 bug UUM-84595: PropertyEditor+Styles..cctor runs via
    // InspectorWindow.ShowButton before DockArea.OldOnGUI sets GUISkin.current,
    // generating N "Unable to use a named GUIStyle" native errors.
    //
    // Important distinction:
    //   Mode A – EditorStyles.s_Current is valid, GUI.skin (native) is null.
    //            Cctor SUCCEEDS; only native log errors are emitted.
    //            This is the original Unity 6 bug this file fixes.
    //
    //   Mode B – EditorStyles.s_Current is null.
    //            Cctor throws NullReferenceException → TypeInitializationException,
    //            permanently breaking PropertyEditor+Styles for the session.
    //            Every subsequent Inspector repaint re-throws TypeInitializationException.
    //
    // Correct fix:
    //   1. Log filter suppresses Mode-A "Unable to use a named GUIStyle" errors
    //      for kFilterFrames ticks (transient startup noise).
    //      Log filter also PERMANENTLY suppresses Mode-B TypeInitializationException
    //      from PropertyEditor+Styles — once the type is broken it spams every frame.
    //      The filter stays in the logger chain for the entire editor session.
    //   2. Best-effort prewarm runs the cctor early (before ShowButton triggers it)
    //      BUT ONLY when EditorStyles is FULLY ready — all key styles non-null.
    //      We never call UpdateSkinCache; partial initialization causes Mode B.
    [InitializeOnLoad]
    internal static class ARSSEditorInit
    {
        private static GUIStyleErrorFilter _activeFilter;
        private static bool _prewarmDone;
        private static int _remainingFrames;
        private const int kFilterFrames = 10;

        static ARSSEditorInit()
        {
            _activeFilter    = InstallLogFilter();
            _prewarmDone     = false;
            _remainingFrames = kFilterFrames;

            TryPrewarm();   // attempt 1: synchronous, before any rendering

            AssemblyReloadEvents.afterAssemblyReload += OnAfterReload;
            EditorApplication.update += OnUpdate;
        }

        private static void OnAfterReload()
        {
            AssemblyReloadEvents.afterAssemblyReload -= OnAfterReload;
            _activeFilter    = InstallLogFilter();
            _prewarmDone     = false;
            _remainingFrames = kFilterFrames;

            TryPrewarm();   // attempt 2: after domain reload completes

            EditorApplication.update += OnUpdate;

            // Unity 6: editors created during the first post-reload rendering pass can
            // show a black inspector because the UIToolkit container is not yet ready.
            // ForceRebuild sets a dirty flag; Unity recreates editors on the NEXT
            // rendering pass (GUIUtility.ProcessEvent), when UIToolkit is properly set up.
            EditorApplication.delayCall += () => ActiveEditorTracker.sharedTracker.ForceRebuild();
        }

        private static void OnUpdate()
        {
            if (!_prewarmDone) TryPrewarm();    // attempts 3+: retry each frame

            if (--_remainingFrames > 0) return;
            EditorApplication.update -= OnUpdate;
            // Stop suppressing Mode-A errors; filter stays in chain for Mode-B suppression.
            _activeFilter?.StopModeASuppression();
            _activeFilter = null;
        }

        // ── Prewarm ───────────────────────────────────────────────────────────────

        // Runs PropertyEditor+Styles..cctor from a safe context so it never
        // runs in InspectorWindow.ShowButton with a null skin.
        // Skipped entirely when EditorStyles is not fully ready — calling
        // UpdateSkinCache or similar to "help" initialization causes Mode B.
        private static void TryPrewarm()
        {
            if (_prewarmDone || !IsEditorStylesFullyReady()) return;

            // Prime GUI.skin so named-style lookups inside the cctor succeed.
            try
            {
                var skin = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector);
                if (skin != null) GUI.skin = skin;
            }
            catch { }

            Type stylesType = null;
            try
            {
                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    stylesType = asm.GetType("UnityEditor.PropertyEditor+Styles");
                    if (stylesType != null) break;
                }
            }
            catch { }

            if (stylesType != null)
            {
                try
                {
                    RuntimeHelpers.RunClassConstructor(stylesType.TypeHandle);
                    _prewarmDone = true;    // only mark done when cctor actually succeeded
                }
                catch { }
            }
        }

        // Returns true only when Unity has already fully initialised EditorStyles —
        // meaning all key style properties are non-null. A false here means the
        // prewarm would run with null GUIStyle values and cause Mode B.
        private static bool IsEditorStylesFullyReady()
        {
            try
            {
                return EditorStyles.label        != null
                    && EditorStyles.boldLabel     != null
                    && EditorStyles.toolbarButton != null
                    && EditorStyles.helpBox       != null;
            }
            catch { return false; }
        }

        // ── Log filter ────────────────────────────────────────────────────────────

        private static GUIStyleErrorFilter InstallLogFilter()
        {
            var current = Debug.unityLogger.logHandler;
            if (current is GUIStyleErrorFilter existing) return existing;
            var filter = new GUIStyleErrorFilter(current);
            Debug.unityLogger.logHandler = filter;
            return filter;
        }

        private sealed class GUIStyleErrorFilter : ILogHandler
        {
            private const string kStyleError   = "Unable to use a named GUIStyle";
            private const string kStylesCctor  = "PropertyEditor+Styles..cctor";
            private readonly ILogHandler _inner;
            private bool _suppressModeA = true;

            public GUIStyleErrorFilter(ILogHandler inner) => _inner = inner;

            // Called after kFilterFrames — Mode-A noise should be gone by then.
            // The filter itself stays in the logger chain to keep suppressing Mode B.
            public void StopModeASuppression() => _suppressModeA = false;

            public void LogFormat(LogType logType, UnityEngine.Object context,
                string format, params object[] args)
            {
                if (_inner == null) return;
                if (_suppressModeA
                    && (logType == LogType.Error || logType == LogType.Assert)
                    && IsGUIStyleError(format, args))
                    return;
                _inner.LogFormat(logType, context, format, args);
            }

            public void LogException(Exception exception, UnityEngine.Object context)
            {
                // Mode B: once PropertyEditor+Styles..cctor throws, the type is
                // permanently broken and re-throws TypeInitializationException on
                // every Inspector repaint. Suppress for the entire editor session.
                if (IsPropertyEditorStylesException(exception)) return;
                _inner?.LogException(exception, context);
            }

            private static bool IsPropertyEditorStylesException(Exception ex)
            {
                for (var e = ex; e != null; e = e.InnerException)
                {
                    if (e.StackTrace?.Contains(kStylesCctor) == true)
                        return true;
                }
                return false;
            }

            private static bool IsGUIStyleError(string format, object[] args)
            {
                if (format?.Contains(kStyleError) == true) return true;
                if (args?.Length > 0 && format != null)
                {
                    try { return string.Format(format, args)?.Contains(kStyleError) == true; }
                    catch { }
                }
                return false;
            }
        }
    }
}
