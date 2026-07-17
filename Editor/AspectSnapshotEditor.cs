using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AspectSwitcher
{
    [CustomEditor(typeof(AspectSnapshotBase), true)]
    public class AspectSnapshotEditor : Editor
    {
        private SerializedProperty _genericEntriesProperty;
        private AspectSnapshotBase _target;

        private void OnEnable()
        {
            _target = (AspectSnapshotBase)target;
            if (target == null) return;

            var targetType = target.GetType();
            var genericEntriesField = GetGenericEntriesField(targetType);

            if (genericEntriesField != null)
                _genericEntriesProperty = serializedObject.FindProperty(genericEntriesField.Name);
        }

        private FieldInfo GetGenericEntriesField(Type type)
        {
            while (type != null && type != typeof(object))
            {
                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(AspectSnapshot<,>))
                    return type.GetField("entries",
                        BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                type = type.BaseType;
            }

            return null;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_switcher"), new GUIContent("Switcher"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_workIfInactive"),
                new GUIContent("Work if GO is inactive"));
            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();

            if (_target.Switcher == null)
            {
                EditorGUILayout.HelpBox(
                    "Assign an AspectRatioStateSwitcher. The snapshot self-registers on Enable.",
                    MessageType.Warning);
                serializedObject.ApplyModifiedProperties();
                return;
            }

            if (_target.Switcher.config == null)
                EditorGUILayout.HelpBox(
                    "The assigned Switcher has no State Config. Add one to the Switcher first.",
                    MessageType.Warning);

            EditorGUILayout.Space(4f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("target"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("transitionOverride"), true);

            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("State Entries", EditorStyles.boldLabel);

            var entriesProp = _genericEntriesProperty;
            var toDelete = -1;

            for (var i = 0; i < entriesProp.arraySize; i++)
                if (!DrawEntry(entriesProp.GetArrayElementAtIndex(i), i))
                    toDelete = i;

            if (toDelete >= 0)
                entriesProp.DeleteArrayElementAtIndex(toDelete);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("+ Add Entry"))
            {
                entriesProp.arraySize++;
                var newElem = entriesProp.GetArrayElementAtIndex(entriesProp.arraySize - 1);
                var statesProp = newElem.FindPropertyRelative("states");
                statesProp.arraySize = 1;
                statesProp.GetArrayElementAtIndex(0).intValue = 0;
            }

            if (GUILayout.Button("Capture All") && _target.target != null)
            {
                serializedObject.ApplyModifiedProperties();
                Undo.RecordObject(_target, "Capture All Snapshots");
                for (var i = 0; i < entriesProp.arraySize; i++)
                    _target.GetDataAt(i)?.CaptureFrom(_target.target);
                EditorUtility.SetDirty(_target);
                serializedObject.Update();
            }

            EditorGUILayout.EndHorizontal();

            serializedObject.ApplyModifiedProperties();
        }

        private bool DrawEntry(SerializedProperty entry, int index)
        {
            if (entry == null) return false;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();

            var statesProp = entry.FindPropertyRelative("states");
            if (statesProp == null)
            {
                EditorGUILayout.HelpBox("Error: 'states' property not found.", MessageType.Error);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                return true;
            }

            if (statesProp.arraySize == 0)
            {
                statesProp.arraySize = 1;
                statesProp.GetArrayElementAtIndex(0).intValue = 0;
            }

            var toRemoveState = -1;
            for (var si = 0; si < statesProp.arraySize; si++)
            {
                var sp = statesProp.GetArrayElementAtIndex(si);
                sp.intValue = (int)(AspectState)EditorGUILayout.EnumPopup(
                    (AspectState)sp.intValue, GUILayout.MinWidth(80f), GUILayout.MaxWidth(150f));

                if (statesProp.arraySize > 1 && GUILayout.Button("✕", GUILayout.Width(18f)))
                    toRemoveState = si;
            }

            if (GUILayout.Button("+ State", GUILayout.Width(56f)))
            {
                statesProp.arraySize++;
                statesProp.GetArrayElementAtIndex(statesProp.arraySize - 1).intValue = 0;
            }

            GUILayout.FlexibleSpace();
            var deleted = GUILayout.Button("✕", GUILayout.Width(22f));
            EditorGUILayout.EndHorizontal();

            if (toRemoveState >= 0)
                statesProp.DeleteArrayElementAtIndex(toRemoveState);

            if (deleted)
            {
                EditorGUILayout.EndVertical();
                return false;
            }

            var dataProp = entry.FindPropertyRelative("_data");

            if (dataProp != null)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(dataProp, new GUIContent("Data"), true);
                EditorGUI.indentLevel--;
            }
            else
            {
                EditorGUILayout.HelpBox($"Error: Serialized field '_data' not found in {entry.type}!",
                    MessageType.Error);
            }

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Capture") && _target.target != null)
            {
                serializedObject.ApplyModifiedProperties();
                Undo.RecordObject(_target, "Capture Snapshot");
                _target.GetDataAt(index).CaptureFrom(_target.target);
                EditorUtility.SetDirty(_target);
                serializedObject.Update();
            }

            if (GUILayout.Button("Preview") && _target.target != null)
            {
                serializedObject.ApplyModifiedProperties();
                Undo.RecordObjects(new Object[] { _target, _target.target }, "Preview Snapshot");
                _target.GetDataAt(index).ApplyTo(_target.target, null, 1f);
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
            return true;
        }
    }
}