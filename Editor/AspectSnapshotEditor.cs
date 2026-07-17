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
    
    float viewWidth = EditorGUIUtility.currentViewWidth - 35f;

    var statesProp = entry.FindPropertyRelative("states");
    if (statesProp == null)
    {
        EditorGUILayout.HelpBox("Error: 'states' property not found.", MessageType.Error);
        EditorGUILayout.EndVertical();
        return true;
    }

    if (statesProp.arraySize == 0)
    {
        statesProp.arraySize = 1;
        statesProp.GetArrayElementAtIndex(0).intValue = 0;
    }

    Rect currentRect = EditorGUILayout.GetControlRect(false, 0f); 
    float currentX = 0f;
    float spaceBetween = 4f;

    EditorGUILayout.BeginHorizontal();
    
    int toRemoveState = -1;
    for (var si = 0; si < statesProp.arraySize; si++)
    {
        float popupWidth = 100f; 
        float removeBtnWidth = (statesProp.arraySize > 1) ? 20f : 0f;
        float elementWidth = popupWidth + removeBtnWidth;

        if (currentX + elementWidth > viewWidth && currentX > 0f)
        {
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(spaceBetween);
            EditorGUILayout.BeginHorizontal();
            currentX = 0f;
        }

        var sp = statesProp.GetArrayElementAtIndex(si);
        
        sp.intValue = (int)(AspectState)EditorGUILayout.EnumPopup(
            (AspectState)sp.intValue, GUILayout.Width(popupWidth));
        currentX += popupWidth;

        if (statesProp.arraySize > 1)
        {
            if (GUILayout.Button("✕", GUILayout.Width(18f), GUILayout.Height(18f)))
                toRemoveState = si;
            currentX += removeBtnWidth;
        }

        currentX += spaceBetween;
    }

    float addStateBtnWidth = 60f;
    if (currentX + addStateBtnWidth > viewWidth && currentX > 0f)
    {
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space(spaceBetween);
        EditorGUILayout.BeginHorizontal();
        currentX = 0f;
    }
    
    if (GUILayout.Button("+ State", GUILayout.Width(addStateBtnWidth)))
    {
        statesProp.arraySize++;
        statesProp.GetArrayElementAtIndex(statesProp.arraySize - 1).intValue = 0;
    }
    currentX += addStateBtnWidth + spaceBetween;

    float deleteEntryBtnWidth = 24f;
    if (currentX + deleteEntryBtnWidth > viewWidth && currentX > 0f)
    {
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space(spaceBetween);
        EditorGUILayout.BeginHorizontal();
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

    EditorGUILayout.Space(4f);

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

    EditorGUILayout.Space(2f);
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