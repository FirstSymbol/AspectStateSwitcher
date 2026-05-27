using UnityEngine;
using UnityEditor;

namespace AspectSwitcher
{
    [CustomPropertyDrawer(typeof(AspectRange))]
    public class AspectRangeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label)
        {
            EditorGUI.BeginProperty(pos, label, prop);

            var minProp = prop.FindPropertyRelative("min");
            var maxProp = prop.FindPropertyRelative("max");

            float lw    = EditorGUIUtility.labelWidth;
            float avail = pos.width - lw - 16f;
            float half  = avail * 0.5f;
            float x     = pos.x + lw;
            float h     = pos.height;
            float y     = pos.y;

            EditorGUI.LabelField(new Rect(pos.x, y, lw, h), label);

            DrawBoundedFloat(new Rect(x,               y, half - 2f, h), minProp, isMin: true);
            EditorGUI.LabelField(new Rect(x + half - 2f, y, 16f,     h), "→");
            DrawBoundedFloat(new Rect(x + half + 14f,   y, half - 2f, h), maxProp, isMin: false);

            EditorGUI.EndProperty();
        }

        private static void DrawBoundedFloat(Rect rect, SerializedProperty prop, bool isMin)
        {
            float val   = prop.floatValue;
            bool  isInf = isMin ? (val < -1e30f) : (val > 1e30f);

            const float btnW = 26f;

            if (isInf)
            {
                if (GUI.Button(rect, isMin ? "-∞" : "+∞", EditorStyles.miniButton))
                    prop.floatValue = isMin ? 0f : 2f;
            }
            else
            {
                float fieldW = rect.width - btnW - 2f;
                prop.floatValue = EditorGUI.FloatField(
                    new Rect(rect.x, rect.y, fieldW, rect.height), val);

                if (GUI.Button(
                    new Rect(rect.x + fieldW + 2f, rect.y, btnW, rect.height),
                    "∞", EditorStyles.miniButton))
                {
                    prop.floatValue = isMin ? float.MinValue : float.MaxValue;
                }
            }
        }
    }
}
