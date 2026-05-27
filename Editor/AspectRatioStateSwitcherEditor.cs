using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace AspectSwitcher
{
    [CustomEditor(typeof(AspectRatioStateSwitcher))]
    public class AspectRatioStateSwitcherEditor : Editor
    {
        private AspectRatioStateSwitcher _target;
        private readonly Dictionary<string, bool> _foldouts = new Dictionary<string, bool>();

        private void OnEnable() => _target = (AspectRatioStateSwitcher)target;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

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

            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("Registered Snapshots", EditorStyles.boldLabel);
            DrawSnapshotsGrouped();

            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("Global Transition", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("globalTransition"), true);

            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("stateStabilization"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("applyOnStart"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("onStateChanged"));

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

        private void DrawSnapshotsGrouped()
        {
            var grouped = GetSnapshotsGrouped();
            if (grouped.Count == 0)
            {
                EditorGUILayout.HelpBox(
                    "No snapshots found. Add an AspectSnapshot component (e.g. UITransformSnapshot) " +
                    "to a scene object and set its Switcher field to this component.",
                    MessageType.None);
                return;
            }

            foreach (var kvp in grouped)
            {
                string key = kvp.Key;
                if (!_foldouts.TryGetValue(key, out bool expanded))
                    _foldouts[key] = expanded = true;

                _foldouts[key] = EditorGUILayout.Foldout(
                    expanded, $"{key}  ({kvp.Value.Count})", true, EditorStyles.foldoutHeader);

                if (!_foldouts[key]) continue;

                EditorGUI.indentLevel++;
                foreach (var c in kvp.Value)
                {
                    if (c == null) continue;
                    EditorGUILayout.BeginHorizontal();
                    using (new EditorGUI.DisabledScope(true))
                        EditorGUILayout.ObjectField(c, typeof(AspectSnapshot), true);
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

        private Dictionary<string, List<AspectSnapshot>> GetSnapshotsGrouped()
        {
            var result = new Dictionary<string, List<AspectSnapshot>>();

            if (Application.isPlaying)
            {
                foreach (var kvp in _target.RegisteredContainers)
                {
                    if (kvp.Value.Count == 0) continue;
                    string key = kvp.Key.Name;
                    result[key] = new List<AspectSnapshot>(kvp.Value);
                }
                return result;
            }

            var all = FindObjectsByType<AspectSnapshot>(FindObjectsInactive.Include);
            foreach (var c in all)
            {
                if (c.Switcher != _target) continue;
                string key = c.GetType().Name;
                if (!result.TryGetValue(key, out var list))
                    result[key] = list = new List<AspectSnapshot>();
                list.Add(c);
            }
            return result;
        }

        private readonly List<AspectState> _previewStates = new List<AspectState>(8);

        private void PreviewState(AspectState state)
        {
            _target.config.GetContainedStates(state, _previewStates);
            if (_previewStates.Count == 0) _previewStates.Add(state);

            if (Application.isPlaying) { _target.ForceState(state); return; }

            var all = FindObjectsByType<AspectSnapshot>(FindObjectsInactive.Include);

            var undoTargets = new List<UnityEngine.Object> { _target };
            foreach (var c in all)
            {
                if (c.Switcher != _target) continue;
                undoTargets.Add(c);
                if (c.target) undoTargets.Add(c.target);
            }
            Undo.RecordObjects(undoTargets.ToArray(), "Preview Aspect State");

            foreach (var c in all)
                if (c.Switcher == _target)
                    c.ApplyStatesInstant(_previewStates);
        }
    }
}
