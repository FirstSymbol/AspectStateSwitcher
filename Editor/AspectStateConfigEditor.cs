using System;
using UnityEngine;
using UnityEditor;

namespace AspectSwitcher
{
    [CustomEditor(typeof(AspectStateConfig))]
    public class AspectStateConfigEditor : Editor
    {
        private AspectStateConfig _target;

        private void OnEnable() => _target = (AspectStateConfig)target;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if (_target.states?.Count > 0)
            {
                var r = GUILayoutUtility.GetRect(0f, AspectRangeDiagram.GetHeight(), GUILayout.ExpandWidth(true));
                AspectRangeDiagram.Draw(r, _target.states);
                EditorGUILayout.Space(4f);
            }

            EditorGUILayout.LabelField("States", EditorStyles.boldLabel);

            var statesProp = serializedObject.FindProperty("states");
            int toDelete = -1;

            for (int i = 0; i < statesProp.arraySize; i++)
            {
                var elem      = statesProp.GetArrayElementAtIndex(i);
                var stateProp = elem.FindPropertyRelative("state");

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.BeginHorizontal();

                stateProp.intValue = (int)(AspectState)EditorGUILayout.EnumPopup(
                    (AspectState)stateProp.intValue,
                    GUILayout.MinWidth(60f), GUILayout.MaxWidth(120f));

                EditorGUILayout.PropertyField(elem.FindPropertyRelative("range"), GUIContent.none);

                if (GUILayout.Button("✕", GUILayout.Width(22f)))
                    toDelete = i;

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
            }

            if (toDelete >= 0)
                statesProp.DeleteArrayElementAtIndex(toDelete);

            if (GUILayout.Button("+ Add State"))
            {
                statesProp.arraySize++;
                var newElem = statesProp.GetArrayElementAtIndex(statesProp.arraySize - 1);
                newElem.FindPropertyRelative("state").intValue = 0;
                newElem.FindPropertyRelative("range").FindPropertyRelative("min").floatValue = float.MinValue;
                newElem.FindPropertyRelative("range").FindPropertyRelative("max").floatValue = float.MaxValue;
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
