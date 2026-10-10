using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace FirstSymbol.AspectRatioSwitcher.Editor
{
  [CustomPropertyDrawer(typeof(AspectRange))]
  internal sealed class AspectRangeDrawer : PropertyDrawer
  {
    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
      SerializedProperty min = property.FindPropertyRelative(nameof(AspectRange.min));
      SerializedProperty max = property.FindPropertyRelative(nameof(AspectRange.max));

      var row = new VisualElement();
      row.AddToClassList("ars-range");
      AspectEditorUtility.ApplyStyles(row);

      var minField = new FloatField { tooltip = "Lower bound (inclusive). 0 = no lower bound." };
      minField.AddToClassList("ars-range__field");
      minField.BindProperty(min);

      var dash = new Label("–");
      dash.AddToClassList("ars-range__dash");

      var maxField = new FloatField { tooltip = "Upper bound (exclusive)." };
      maxField.AddToClassList("ars-range__field");
      var infinite = new Label("∞");
      infinite.AddToClassList("ars-range__infinite");

      var unbounded = new Toggle { text = "∞", tooltip = "No upper bound" };
      unbounded.AddToClassList("ars-range__toggle");

      void Refresh()
      {
        bool isUnbounded = float.IsPositiveInfinity(max.floatValue);
        unbounded.SetValueWithoutNotify(isUnbounded);
        maxField.style.display = isUnbounded ? DisplayStyle.None : DisplayStyle.Flex;
        infinite.style.display = isUnbounded ? DisplayStyle.Flex : DisplayStyle.None;
        if (!isUnbounded)
          maxField.SetValueWithoutNotify(max.floatValue);
      }

      maxField.RegisterValueChangedCallback(evt =>
      {
        max.floatValue = Mathf.Max(0f, evt.newValue);
        max.serializedObject.ApplyModifiedProperties();
      });

      unbounded.RegisterValueChangedCallback(evt =>
      {
        max.floatValue = evt.newValue ? AspectRange.Unbounded : Mathf.Max(min.floatValue + 0.25f, 2f);
        max.serializedObject.ApplyModifiedProperties();
        Refresh();
      });

      row.TrackPropertyValue(max, _ => Refresh());
      Refresh();

      row.Add(minField);
      row.Add(dash);
      row.Add(maxField);
      row.Add(infinite);
      row.Add(unbounded);
      return row;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
      SerializedProperty min = property.FindPropertyRelative(nameof(AspectRange.min));
      SerializedProperty max = property.FindPropertyRelative(nameof(AspectRange.max));
      position = EditorGUI.PrefixLabel(position, label);
      float width = (position.width - 40f) / 2f;
      EditorGUI.PropertyField(new Rect(position.x, position.y, width, position.height), min, GUIContent.none);
      bool unbounded = float.IsPositiveInfinity(max.floatValue);
      if (!unbounded)
        EditorGUI.PropertyField(new Rect(position.x + width + 4f, position.y, width, position.height), max, GUIContent.none);
      else
        EditorGUI.LabelField(new Rect(position.x + width + 4f, position.y, width, position.height), "∞");
      bool toggled = GUI.Toggle(new Rect(position.xMax - 32f, position.y, 32f, position.height), unbounded, "∞", EditorStyles.miniButton);
      if (toggled != unbounded)
        max.floatValue = toggled ? AspectRange.Unbounded : Mathf.Max(min.floatValue + 0.25f, 2f);
    }
  }

  [CustomPropertyDrawer(typeof(AspectStateDefinition))]
  internal sealed class AspectStateDefinitionDrawer : PropertyDrawer
  {
    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
      var row = new VisualElement();
      row.AddToClassList("ars-state-row");
      AspectEditorUtility.ApplyStyles(row);

      var color = new ColorField { showAlpha = false, showEyeDropper = false };
      color.AddToClassList("ars-state-row__color");
      color.BindProperty(property.FindPropertyRelative(nameof(AspectStateDefinition.color)));

      var stateName = new TextField();
      stateName.AddToClassList("ars-state-row__name");
      stateName.BindProperty(property.FindPropertyRelative(nameof(AspectStateDefinition.name)));

      var range = new PropertyField(property.FindPropertyRelative(nameof(AspectStateDefinition.range)), string.Empty);
      range.AddToClassList("ars-state-row__range");
      range.BindProperty(property.FindPropertyRelative(nameof(AspectStateDefinition.range)));

      row.Add(color);
      row.Add(stateName);
      row.Add(range);
      return row;
    }
  }

  [CustomPropertyDrawer(typeof(TransitionSettings))]
  internal sealed class TransitionSettingsDrawer : PropertyDrawer
  {
    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
      var root = new VisualElement();
      SerializedProperty mode = property.FindPropertyRelative(nameof(TransitionSettings.mode));
      var modeField = new PropertyField(mode, property.displayName);
      root.Add(modeField);

      var details = new VisualElement();
      details.style.marginLeft = 12;
      details.Add(new PropertyField(property.FindPropertyRelative(nameof(TransitionSettings.duration))));
      details.Add(new PropertyField(property.FindPropertyRelative(nameof(TransitionSettings.curve))));
      details.Add(new PropertyField(property.FindPropertyRelative(nameof(TransitionSettings.unscaledTime))));
      root.Add(details);

      void Refresh(SerializedProperty p) =>
        details.style.display = p.enumValueIndex == (int)TransitionMode.Tween ? DisplayStyle.Flex : DisplayStyle.None;

      Refresh(mode);
      root.TrackPropertyValue(mode, Refresh);
      return root;
    }
  }
}
