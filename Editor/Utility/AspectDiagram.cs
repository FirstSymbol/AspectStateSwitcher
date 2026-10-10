using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace FirstSymbol.AspectRatioSwitcher.Editor
{
  internal enum AspectDiagramMode
  {
    /// <summary>Ranges as bars on a logarithmic aspect axis.</summary>
    Bars,

    /// <summary>Each state as a screen thumbnail with its real proportions.</summary>
    Screens
  }

  /// <summary>
  /// Visual map of profile states. <see cref="AspectDiagramMode.Bars"/> shows ranges on a logarithmic axis
  /// (overlapping states on separate lanes; when <see cref="Editable"/>, edges of the selected state can be dragged).
  /// <see cref="AspectDiagramMode.Screens"/> shows every state as a screen of its typical proportions with
  /// outlines of its narrowest and widest screens.
  /// </summary>
  internal sealed class AspectDiagram : IMGUIContainer
  {
    private const string ModeKey = "FirstSymbol.AspectRatioSwitcher.DiagramMode";
    private const float HeaderHeight = 22f;
    private const float CardWidth = 112f;
    private const float CardHeight = 122f;
    private const float CardGap = 6f;
    private const float ShapeWidth = 88f;
    private const float ShapeHeight = 68f;
    private const float MinAspect = 0.4f;
    private const float MaxAspect = 2.8f;
    private const float LaneHeight = 20f;
    private const float LaneGap = 3f;
    private const float AxisHeight = 18f;
    private const float Padding = 6f;
    private const float EdgeGrab = 5f;

    private static readonly (float aspect, string label)[] Ticks =
    {
      (9f / 21f, "9:21"), (9f / 16f, "9:16"), (3f / 4f, "3:4"), (1f, "1:1"),
      (4f / 3f, "4:3"), (16f / 10f, "16:10"), (16f / 9f, "16:9"), (21f / 9f, "21:9")
    };

    private readonly List<(AspectStateDefinition state, int lane)> _layout = new();
    private readonly List<(Rect rect, AspectStateDefinition state)> _cards = new();
    private int _laneCount;
    private AspectStateDefinition _dragState;
    private bool _dragMax;

    public AspectDiagram()
    {
      onGUIHandler = Draw;
      AddToClassList("ars-diagram");
    }

    public AspectProfile Profile { get; set; }
    public bool Editable { get; set; }
    public string SelectedId { get; set; }

    /// <summary>Aspect drawn as a vertical marker (e.g. the Game view aspect).</summary>
    public float? Marker { get; set; }

    public string MarkerLabel { get; set; } = "Game";

    /// <summary>Returns true for states drawn as active.</summary>
    public Func<AspectStateDefinition, bool> IsActive { get; set; }

    public event Action<AspectStateDefinition> StateClicked;

    /// <summary>Display mode shared by every diagram (remembered between sessions).</summary>
    public static AspectDiagramMode Mode
    {
      get => (AspectDiagramMode)EditorPrefs.GetInt(ModeKey, (int)AspectDiagramMode.Screens);
      set => EditorPrefs.SetInt(ModeKey, (int)value);
    }

    private void Draw()
    {
      if (Profile == null)
        return;

      float width = contentRect.width > 1f ? contentRect.width : 300f;
      AspectDiagramMode mode = Mode;
      float bodyHeight;
      if (mode == AspectDiagramMode.Bars)
      {
        LayoutLanes();
        bodyHeight = Padding * 2f + _laneCount * (LaneHeight + LaneGap) + AxisHeight;
      }
      else
      {
        bodyHeight = ScreensHeight(width);
      }

      float height = HeaderHeight + bodyHeight;
      if (Math.Abs(style.height.value.value - height) > 0.5f)
        style.height = height;

      Rect rect = GUILayoutUtility.GetRect(width, height);
      rect.width = width;
      EditorGUI.DrawRect(rect, EditorGUIUtility.isProSkin ? new Color(0.13f, 0.13f, 0.13f) : new Color(0.82f, 0.82f, 0.82f));
      DrawHeader(new Rect(rect.x, rect.y, rect.width, HeaderHeight), mode);

      var body = new Rect(rect.x, rect.y + HeaderHeight, rect.width, bodyHeight);
      if (mode == AspectDiagramMode.Bars)
      {
        var plot = new Rect(body.x + Padding, body.y + Padding, body.width - Padding * 2f, body.height - Padding * 2f);
        DrawTicks(plot);
        DrawStates(plot);
        DrawMarker(plot);
        HandleInput(plot);
      }
      else
      {
        DrawScreens(body);
      }
    }

    private void DrawHeader(Rect rect, AspectDiagramMode mode)
    {
      var hint = new GUIStyle(EditorStyles.miniLabel) { alignment = TextAnchor.MiddleLeft };
      string text = mode == AspectDiagramMode.Bars
        ? Editable ? "Click a state, drag its edges to edit" : "Aspect = width / height"
        : "Filled: typical screen, outlines: narrowest and widest";
      GUI.Label(new Rect(rect.x + Padding, rect.y + 2f, rect.width - 140f, rect.height - 2f), text, hint);

      var bars = new Rect(rect.xMax - 124f - Padding, rect.y + 3f, 62f, 16f);
      var screens = new Rect(bars.xMax, bars.y, 62f, 16f);
      if (GUI.Toggle(bars, mode == AspectDiagramMode.Bars, "Bars", EditorStyles.miniButtonLeft) && mode != AspectDiagramMode.Bars)
      {
        Mode = AspectDiagramMode.Bars;
        MarkDirtyRepaint();
      }

      if (GUI.Toggle(screens, mode == AspectDiagramMode.Screens, "Screens", EditorStyles.miniButtonRight) && mode != AspectDiagramMode.Screens)
      {
        Mode = AspectDiagramMode.Screens;
        MarkDirtyRepaint();
      }
    }

    #region Screens

    private int CardCount
    {
      get
      {
        int count = Marker.HasValue ? 1 : 0;
        foreach (AspectStateDefinition state in Profile.States)
          if (state != null && state.range.IsValid)
            count++;
        return count;
      }
    }

    private static int CardsPerRow(float width) =>
      Mathf.Max(1, Mathf.FloorToInt((width - Padding * 2f + CardGap) / (CardWidth + CardGap)));

    private float ScreensHeight(float width)
    {
      int rows = Mathf.Max(1, Mathf.CeilToInt(CardCount / (float)CardsPerRow(width)));
      return Padding * 2f + rows * CardHeight + (rows - 1) * CardGap;
    }

    private void DrawScreens(Rect body)
    {
      _cards.Clear();
      int perRow = CardsPerRow(body.width);
      float rowWidth = perRow * CardWidth + (perRow - 1) * CardGap;
      float startX = body.x + Mathf.Max(Padding, (body.width - rowWidth) / 2f);
      int index = 0;

      Rect CardRect(int i) => new(
        startX + i % perRow * (CardWidth + CardGap),
        body.y + Padding + i / perRow * (CardHeight + CardGap),
        CardWidth, CardHeight);

      foreach (AspectStateDefinition state in Profile.States)
      {
        if (state == null || !state.range.IsValid)
          continue;

        Rect card = CardRect(index++);
        _cards.Add((card, state));
        DrawStateScreen(card, state);
      }

      if (Marker.HasValue)
        DrawMarkerScreen(CardRect(index));

      Event e = Event.current;
      if (e.type != EventType.MouseDown || e.button != 0)
        return;

      foreach ((Rect rect, AspectStateDefinition state) in _cards)
      {
        if (!rect.Contains(e.mousePosition))
          continue;
        SelectedId = state.Id;
        StateClicked?.Invoke(state);
        e.Use();
        MarkDirtyRepaint();
        return;
      }
    }

    private void DrawStateScreen(Rect card, AspectStateDefinition state)
    {
      bool selected = state.Id == SelectedId;
      bool active = IsActive?.Invoke(state) ?? false;
      Color color = state.color;

      Color background = selected ? new Color(color.r, color.g, color.b, 0.22f) : new Color(0f, 0f, 0f, 0.18f);
      RoundedRect(card, background, 0f, 6f);
      if (selected || active)
        RoundedRect(card, selected ? Color.white : new Color(color.r, color.g, color.b, 0.9f), selected ? 2f : 1.5f, 6f);

      var shapeBox = new Rect(card.x + (card.width - ShapeWidth) / 2f, card.y + 8f, ShapeWidth, ShapeHeight);
      var outline = new Color(color.r, color.g, color.b, 0.55f);
      if (state.range.HasLowerBound)
        RoundedRect(Shape(shapeBox, Mathf.Max(state.range.min, MinAspect)), outline, 1f, 3f);
      if (state.range.HasUpperBound)
        RoundedRect(Shape(shapeBox, Mathf.Min(state.range.max, MaxAspect)), outline, 1f, 3f);

      Rect screen = Shape(shapeBox, state.range.Center);
      RoundedRect(screen, new Color(color.r, color.g, color.b, active || selected ? 0.95f : 0.7f), 0f, 4f);
      // A thin dark bezel makes the thumbnail read as a screen.
      RoundedRect(screen, new Color(0f, 0f, 0f, 0.45f), 2f, 4f);

      var nameStyle = new GUIStyle(EditorStyles.miniBoldLabel) { alignment = TextAnchor.MiddleCenter, clipping = TextClipping.Clip };
      var rangeStyle = new GUIStyle(EditorStyles.miniLabel) { alignment = TextAnchor.MiddleCenter, clipping = TextClipping.Clip };
      GUI.Label(new Rect(card.x + 2f, shapeBox.yMax + 6f, card.width - 4f, 16f), active ? "● " + state.name : state.name, nameStyle);
      GUI.Label(new Rect(card.x + 2f, shapeBox.yMax + 21f, card.width - 4f, 14f), state.range.ToString(), rangeStyle);
    }

    private void DrawMarkerScreen(Rect card)
    {
      var red = new Color(0.95f, 0.3f, 0.3f);
      RoundedRect(card, new Color(0f, 0f, 0f, 0.18f), 0f, 6f);

      var shapeBox = new Rect(card.x + (card.width - ShapeWidth) / 2f, card.y + 8f, ShapeWidth, ShapeHeight);
      Rect screen = Shape(shapeBox, Mathf.Clamp(Marker.Value, MinAspect, MaxAspect));
      RoundedRect(screen, new Color(0.08f, 0.08f, 0.08f, 1f), 0f, 4f);
      RoundedRect(screen, red, 2f, 4f);

      var nameStyle = new GUIStyle(EditorStyles.miniBoldLabel) { alignment = TextAnchor.MiddleCenter };
      nameStyle.normal.textColor = red;
      var valueStyle = new GUIStyle(EditorStyles.miniLabel) { alignment = TextAnchor.MiddleCenter };
      GUI.Label(new Rect(card.x, shapeBox.yMax + 6f, card.width, 16f), MarkerLabel == "Game" ? "Game view" : MarkerLabel, nameStyle);
      GUI.Label(new Rect(card.x, shapeBox.yMax + 21f, card.width, 14f), AspectRange.Format(Marker.Value), valueStyle);
    }

    /// <summary>Largest rect of the aspect that fits the box, centered.</summary>
    private static Rect Shape(Rect box, float aspect)
    {
      float width = box.width;
      float height = width / aspect;
      if (height > box.height)
      {
        height = box.height;
        width = height * aspect;
      }

      return new Rect(box.center.x - width / 2f, box.center.y - height / 2f, width, height);
    }

    private static void RoundedRect(Rect rect, Color color, float borderWidth, float radius) =>
      GUI.DrawTexture(rect, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0f, color, borderWidth, radius);

    #endregion

    private void LayoutLanes()
    {
      _layout.Clear();
      var lanes = new List<List<AspectRange>>();
      foreach (AspectStateDefinition state in Profile.States)
      {
        if (state == null || !state.range.IsValid)
          continue;

        int lane = 0;
        while (lane < lanes.Count && lanes[lane].Exists(r => r.Overlaps(state.range)))
          lane++;
        if (lane == lanes.Count)
          lanes.Add(new List<AspectRange>());

        lanes[lane].Add(state.range);
        _layout.Add((state, lane));
      }

      _laneCount = Mathf.Max(1, lanes.Count);
    }

    private void DrawTicks(Rect plot)
    {
      Color line = EditorGUIUtility.isProSkin ? new Color(1f, 1f, 1f, 0.07f) : new Color(0f, 0f, 0f, 0.1f);
      var labelStyle = new GUIStyle(EditorStyles.miniLabel) { alignment = TextAnchor.UpperCenter };
      float axisY = plot.yMax - AxisHeight + 2f;

      foreach ((float aspect, string label) in Ticks)
      {
        float x = ToX(plot, aspect);
        EditorGUI.DrawRect(new Rect(x, plot.y, 1f, plot.height - AxisHeight), line);
        GUI.Label(new Rect(x - 20f, axisY, 40f, AxisHeight), label, labelStyle);
      }
    }

    private void DrawStates(Rect plot)
    {
      var labelStyle = new GUIStyle(EditorStyles.miniBoldLabel) { alignment = TextAnchor.MiddleCenter, clipping = TextClipping.Clip };

      foreach ((AspectStateDefinition state, int lane) in _layout)
      {
        Rect r = StateRect(plot, state, lane);
        bool selected = state.Id == SelectedId;
        bool active = IsActive?.Invoke(state) ?? false;

        Color color = state.color;
        color.a = selected || active ? 1f : 0.6f;
        EditorGUI.DrawRect(r, color);

        if (active || selected)
        {
          Color outline = selected ? Color.white : new Color(1f, 1f, 1f, 0.6f);
          float w = selected ? 2f : 1f;
          EditorGUI.DrawRect(new Rect(r.x, r.y, r.width, w), outline);
          EditorGUI.DrawRect(new Rect(r.x, r.yMax - w, r.width, w), outline);
          EditorGUI.DrawRect(new Rect(r.x, r.y, w, r.height), outline);
          EditorGUI.DrawRect(new Rect(r.xMax - w, r.y, w, r.height), outline);
        }

        if (r.width > 24f)
        {
          labelStyle.normal.textColor = AspectEditorUtility.ReadableTextColor(state.color);
          GUI.Label(r, r.width > 70f ? $"{state.name}" : Short(state.name), labelStyle);
        }

        if (Editable && selected)
        {
          if (state.range.HasLowerBound)
            EditorGUIUtility.AddCursorRect(EdgeRect(r, false), MouseCursor.ResizeHorizontal);
          if (state.range.HasUpperBound)
            EditorGUIUtility.AddCursorRect(EdgeRect(r, true), MouseCursor.ResizeHorizontal);
        }
      }
    }

    private void DrawMarker(Rect plot)
    {
      if (!Marker.HasValue)
        return;

      float x = ToX(plot, Mathf.Clamp(Marker.Value, MinAspect, MaxAspect));
      var red = new Color(0.95f, 0.3f, 0.3f);
      EditorGUI.DrawRect(new Rect(x - 1f, plot.y - 2f, 2f, plot.height - AxisHeight + 4f), red);
      var style = new GUIStyle(EditorStyles.miniBoldLabel) { normal = { textColor = red } };
      string text = $"{MarkerLabel} {AspectRange.Format(Marker.Value)}";
      float width = style.CalcSize(new GUIContent(text)).x;
      float labelX = x + 3f + width > plot.xMax ? x - width - 3f : x + 3f;
      GUI.Label(new Rect(labelX, plot.yMax - AxisHeight - 14f, width, 14f), text, style);
    }

    private void HandleInput(Rect plot)
    {
      Event e = Event.current;
      switch (e.type)
      {
        case EventType.MouseDown when e.button == 0 && plot.Contains(e.mousePosition):
        {
          if (Editable && TryBeginDrag(plot, e.mousePosition))
          {
            e.Use();
            return;
          }

          foreach ((AspectStateDefinition state, int lane) in _layout)
          {
            if (!StateRect(plot, state, lane).Contains(e.mousePosition))
              continue;
            SelectedId = state.Id;
            StateClicked?.Invoke(state);
            e.Use();
            MarkDirtyRepaint();
            return;
          }

          break;
        }
        case EventType.MouseDrag when _dragState != null:
        {
          float aspect = Mathf.Round(FromX(plot, e.mousePosition.x) * 100f) / 100f;
          if (_dragMax)
            _dragState.range.max = Mathf.Max(aspect, _dragState.range.min + 0.01f);
          else
            _dragState.range.min = Mathf.Clamp(aspect, 0.01f, _dragState.range.max - 0.01f);
          EditorUtility.SetDirty(Profile);
          Profile.NotifyChanged();
          e.Use();
          MarkDirtyRepaint();
          break;
        }
        case EventType.MouseUp when _dragState != null:
          _dragState = null;
          e.Use();
          break;
      }
    }

    private bool TryBeginDrag(Rect plot, Vector2 mouse)
    {
      foreach ((AspectStateDefinition state, int lane) in _layout)
      {
        if (state.Id != SelectedId)
          continue;

        Rect r = StateRect(plot, state, lane);
        bool onMax = state.range.HasUpperBound && EdgeRect(r, true).Contains(mouse);
        bool onMin = state.range.HasLowerBound && EdgeRect(r, false).Contains(mouse);
        if (!onMax && !onMin)
          return false;

        Undo.RecordObject(Profile, "Edit Aspect Range");
        _dragState = state;
        _dragMax = onMax;
        return true;
      }

      return false;
    }

    private Rect StateRect(Rect plot, AspectStateDefinition state, int lane)
    {
      float x0 = state.range.HasLowerBound ? ToX(plot, Mathf.Max(state.range.min, MinAspect)) : plot.x;
      float x1 = state.range.HasUpperBound ? ToX(plot, Mathf.Min(state.range.max, MaxAspect)) : plot.xMax;
      float y = plot.y + lane * (LaneHeight + LaneGap);
      return new Rect(x0, y, Mathf.Max(2f, x1 - x0 - 1f), LaneHeight);
    }

    private static Rect EdgeRect(Rect r, bool max) =>
      new(max ? r.xMax - EdgeGrab : r.x - EdgeGrab, r.y, EdgeGrab * 2f, r.height);

    private static float ToX(Rect plot, float aspect)
    {
      float t = (Mathf.Log(aspect) - Mathf.Log(MinAspect)) / (Mathf.Log(MaxAspect) - Mathf.Log(MinAspect));
      return plot.x + Mathf.Clamp01(t) * plot.width;
    }

    private static float FromX(Rect plot, float x)
    {
      float t = Mathf.Clamp01((x - plot.x) / plot.width);
      return Mathf.Exp(Mathf.Log(MinAspect) + t * (Mathf.Log(MaxAspect) - Mathf.Log(MinAspect)));
    }

    private static string Short(string name) => string.IsNullOrEmpty(name) ? string.Empty : name.Substring(0, 1);
  }
}
