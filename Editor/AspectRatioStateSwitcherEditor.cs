using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace AspectSwitcher
{
    [CustomEditor(typeof(AspectRatioStateSwitcher))]
    public class AspectRatioStateSwitcherEditor : Editor
    {
        private AspectRatioStateSwitcher _target;
        private readonly Dictionary<SnapshotType, bool> _foldouts = new Dictionary<SnapshotType, bool>();

        private void OnEnable() => _target = (AspectRatioStateSwitcher)target;

        public override void OnInspectorGUI()
        {
            if (GUI.skin == null) { Repaint(); return; }
            serializedObject.Update();

            // ── Config ──────────────────────────────────────────────────────────────
            EditorGUILayout.LabelField("Configuration", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("config"), new GUIContent("State Config"));

            if (_target.config == null)
            {
                EditorGUILayout.HelpBox(
                    "Assign an Aspect State Config asset.\n" +
                    "Create one: right-click in Project → Create → ARSS → Aspect State Config",
                    MessageType.Info);
            }
            else if (_target.config.states?.Count > 0)
            {
                var r = GUILayoutUtility.GetRect(0f, AspectRangeDiagram.GetHeight(), GUILayout.ExpandWidth(true));
                AspectRangeDiagram.Draw(r, _target.config.states);
                EditorGUILayout.Space(2f);
            }

            // ── Registered containers ───────────────────────────────────────────────
            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("Registered Containers", EditorStyles.boldLabel);
            DrawContainersGrouped();

            // ── Global Transition ───────────────────────────────────────────────────
            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("Global Transition", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("globalTransition"), true);

            // ── Settings ────────────────────────────────────────────────────────────
            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("stateStabilization"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("applyOnStart"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("onStateChanged"));

            // ── Preview ─────────────────────────────────────────────────────────────
            if (_target.config?.states?.Count > 0)
            {
                EditorGUILayout.Space(6f);
                EditorGUILayout.LabelField("Preview State", EditorStyles.boldLabel);
                EditorGUILayout.BeginHorizontal();
                foreach (var s in _target.config.states)
                    if (GUILayout.Button(s.state.ToString()))
                        PreviewState(s.state);
                EditorGUILayout.EndHorizontal();
                if (!Application.isPlaying)
                    EditorGUILayout.HelpBox("Applies snapshots in Edit Mode (supports Undo).", MessageType.None);
            }

            serializedObject.ApplyModifiedProperties();
        }

        // ── Container display ────────────────────────────────────────────────────

        private void DrawContainersGrouped()
        {
            var grouped = GetContainersGrouped();
            if (grouped.Count == 0)
            {
                EditorGUILayout.HelpBox(
                    "No containers found. Add AspectSnapshotContainer components to scene objects " +
                    "and set their Switcher field to this component.",
                    MessageType.None);
                return;
            }

            foreach (var kvp in grouped)
            {
                if (!_foldouts.TryGetValue(kvp.Key, out bool expanded))
                    _foldouts[kvp.Key] = expanded = true;

                _foldouts[kvp.Key] = EditorGUILayout.Foldout(
                    expanded, $"{kvp.Key}  ({kvp.Value.Count})", true, EditorStyles.foldoutHeader);

                if (!_foldouts[kvp.Key]) continue;

                EditorGUI.indentLevel++;
                foreach (var c in kvp.Value)
                {
                    if (c == null) continue;
                    EditorGUILayout.BeginHorizontal();

                    using (new EditorGUI.DisabledScope(true))
                        EditorGUILayout.ObjectField(c, typeof(AspectSnapshotContainer), true);

                    if (GUILayout.Button("→", GUILayout.Width(24f)))
                    {
                        Selection.activeGameObject = c.gameObject;
                        EditorGUIUtility.PingObject(c.gameObject);
                    }
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUI.indentLevel--;
            }
        }

        private Dictionary<SnapshotType, List<AspectSnapshotContainer>> GetContainersGrouped()
        {
            var result = new Dictionary<SnapshotType, List<AspectSnapshotContainer>>();

            if (Application.isPlaying)
            {
                foreach (var kvp in _target.RegisteredContainers)
                    if (kvp.Value.Count > 0)
                        result[kvp.Key] = new List<AspectSnapshotContainer>(kvp.Value);
                return result;
            }

            // Edit mode: scan the scene for containers that reference this switcher.
            var all = FindObjectsByType<AspectSnapshotContainer>(
                FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var c in all)
            {
                if (c.Switcher != _target) continue;
                if (!result.TryGetValue(c.type, out var list))
                    result[c.type] = list = new List<AspectSnapshotContainer>();
                list.Add(c);
            }
            return result;
        }

        // ── Preview ──────────────────────────────────────────────────────────────

        private void PreviewState(AspectState state)
        {
            if (Application.isPlaying) { _target.ForceState(state); return; }

            var all = FindObjectsByType<AspectSnapshotContainer>(
                FindObjectsInactive.Include, FindObjectsSortMode.None);

            var undoTargets = new System.Collections.Generic.List<Object> { _target };
            foreach (var c in all)
            {
                if (c.Switcher != _target) continue;
                undoTargets.Add(c);
                if (c.target) undoTargets.Add(c.target);
            }
            Undo.RecordObjects(undoTargets.ToArray(), "Preview Aspect State");

            foreach (var c in all)
                if (c.Switcher == _target)
                    c.ApplyStateInstant(state);
        }
    }
}
