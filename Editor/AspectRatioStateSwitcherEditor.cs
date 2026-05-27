using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace AspectSwitcher
{
    [CustomEditor(typeof(AspectRatioStateSwitcher))]
    public class AspectRatioStateSwitcherEditor : Editor
    {
        private AspectRatioStateSwitcher _target;

        private void OnEnable() => _target = (AspectRatioStateSwitcher)target;

        public override void OnInspectorGUI()
        {
            if (GUI.skin == null) { Repaint(); return; }
            serializedObject.Update();

            // ── Config ──────────────────────────────────────────────────────────────
            EditorGUILayout.LabelField("Configuration", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("config"),
                new GUIContent("State Config"));

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

            // ── Targets ─────────────────────────────────────────────────────────────
            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("Controlled Containers", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("targets"), true);

            // ── Global Transition ───────────────────────────────────────────────────
            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("Global Transition", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("globalTransition"), true);

            // ── Settings ────────────────────────────────────────────────────────────
            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("checkInterval"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("applyOnStart"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("onStateChanged"));

            // ── Preview ─────────────────────────────────────────────────────────────
            if (_target.config?.states?.Count > 0)
            {
                EditorGUILayout.Space(6f);
                EditorGUILayout.LabelField("Preview State", EditorStyles.boldLabel);
                EditorGUILayout.BeginHorizontal();
                foreach (var s in _target.config.states)
                {
                    if (GUILayout.Button(s.state.ToString()))
                        PreviewState(s.state);
                }
                EditorGUILayout.EndHorizontal();
                if (!Application.isPlaying)
                    EditorGUILayout.HelpBox("Applies snapshots in Edit Mode (supports Undo).", MessageType.None);
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void PreviewState(AspectState state)
        {
            if (Application.isPlaying) { _target.ForceState(state); return; }

            var undoList = new List<UnityEngine.Object> { _target };
            foreach (var c in _target.targets)
            {
                if (c == null) continue;
                undoList.Add(c);
                if (c.target) undoList.Add(c.target);
            }
            Undo.RecordObjects(undoList.ToArray(), "Preview Aspect State");

            foreach (var c in _target.targets)
                c?.ApplyStateInstant(state);
        }
    }
}
