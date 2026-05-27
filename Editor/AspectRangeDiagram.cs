using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace AspectSwitcher
{
    public static class AspectRangeDiagram
    {
        private static readonly Color[] StateColors =
        {
            new Color(0.30f, 0.60f, 1.00f, 0.75f),
            new Color(0.30f, 1.00f, 0.50f, 0.75f),
            new Color(1.00f, 0.70f, 0.30f, 0.75f),
            new Color(0.90f, 0.30f, 0.55f, 0.75f),
            new Color(0.70f, 0.40f, 1.00f, 0.75f),
            new Color(0.30f, 0.90f, 0.90f, 0.75f),
        };

        private static readonly float[] Markers = { 0.5f, 0.75f, 1.0f, 1.333f, 1.778f, 2.0f, 3.0f };
        private const float DiagramMin  = 0.3f;
        private const float DiagramMax  = 3.2f;
        private const float BarHeight   = 36f;
        private const float LabelHeight = 14f;

        private static GUIStyle _stateNameStyle;
        private static GUIStyle _markerStyle;

        private static GUIStyle StateNameStyle => _stateNameStyle ??= new GUIStyle
        {
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold,
            fontSize  = 9,
            normal    = { textColor = Color.white }
        };

        private static GUIStyle MarkerStyle => _markerStyle ??= new GUIStyle
        {
            alignment = TextAnchor.UpperCenter,
            fontSize  = 8,
            normal    = { textColor = new Color(0.65f, 0.65f, 0.65f) }
        };

        public static float GetHeight() => BarHeight + LabelHeight + 6f;

        public static void Draw(Rect rect, List<AspectStateDefinition> states)
        {
            float totalRange = DiagramMax - DiagramMin;
            float barY       = rect.y + 2f;

            EditorGUI.DrawRect(new Rect(rect.x, barY, rect.width, BarHeight),
                new Color(0.12f, 0.12f, 0.12f, 1f));

            for (int i = 0; i < states.Count; i++)
            {
                var s  = states[i];
                if (!s.showInDiagram) continue;
                float mn = s.range.min < -1e30f ? DiagramMin : Mathf.Clamp(s.range.min, DiagramMin, DiagramMax);
                float mx = s.range.max >  1e30f ? DiagramMax : Mathf.Clamp(s.range.max, DiagramMin, DiagramMax);
                if (mx <= mn) continue;

                float x = rect.x + (mn - DiagramMin) / totalRange * rect.width;
                float w = (mx - mn) / totalRange * rect.width;

                EditorGUI.DrawRect(new Rect(x + 1f, barY + 2f, w - 2f, BarHeight - 4f),
                    StateColors[i % StateColors.Length]);

                if (w > 24f)
                {
                    GUI.Label(new Rect(x + 2f, barY + 2f, w - 4f, BarHeight - 4f),
                        s.state.ToString(), StateNameStyle);
                }
            }

            float markerY = barY + BarHeight;
            foreach (float m in Markers)
            {
                float x = rect.x + (m - DiagramMin) / totalRange * rect.width;
                EditorGUI.DrawRect(new Rect(x, barY, 1f, BarHeight),
                    new Color(0.5f, 0.5f, 0.5f, 0.4f));
                GUI.Label(new Rect(x - 14f, markerY, 28f, LabelHeight),
                    m.ToString("0.##"), MarkerStyle);
            }
        }
    }
}
