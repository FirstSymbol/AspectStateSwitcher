using System;
using UnityEngine;
using UnityEngine.UI;

namespace AspectSwitcher
{
    [Serializable]
    public class GridLayoutGroupData : SnapshotData
    {
        public Vector2 cellSize = new Vector2(100f, 100f);
        public Vector2 spacing;
        public RectOffset padding = new RectOffset();
        public GridLayoutGroup.Corner startCorner = GridLayoutGroup.Corner.UpperLeft;
        public GridLayoutGroup.Axis startAxis = GridLayoutGroup.Axis.Horizontal;
        public TextAnchor childAlignment = TextAnchor.UpperLeft;
        public GridLayoutGroup.Constraint constraint = GridLayoutGroup.Constraint.Flexible;
        public int constraintCount = 2;

        public override void CaptureFrom(Component target)
        {
            if (!(target is GridLayoutGroup grid)) return;

            cellSize = grid.cellSize;
            spacing = grid.spacing;
            padding = CopyPadding(grid.padding);
            startCorner = grid.startCorner;
            startAxis = grid.startAxis;
            childAlignment = grid.childAlignment;
            constraint = grid.constraint;
            constraintCount = grid.constraintCount;
        }

        public override void ApplyTo(Component target, SnapshotData previousStateData, float t)
        {
            if (!(target is GridLayoutGroup grid)) return;

            var source = previousStateData as GridLayoutGroupData;
            Vector2 fromCellSize = source?.cellSize ?? grid.cellSize;
            Vector2 fromSpacing = source?.spacing ?? grid.spacing;

            grid.cellSize = t < 1f ? Vector2.Lerp(fromCellSize, cellSize, t) : cellSize;
            grid.spacing = t < 1f ? Vector2.Lerp(fromSpacing, spacing, t) : spacing;

            if (t < 1f) return;

            grid.padding = CopyPadding(padding);
            grid.startCorner = startCorner;
            grid.startAxis = startAxis;
            grid.childAlignment = childAlignment;
            grid.constraint = constraint;
            grid.constraintCount = Mathf.Max(1, constraintCount);
        }

        private static RectOffset CopyPadding(RectOffset source)
        {
            return source == null
                ? new RectOffset()
                : new RectOffset(source.left, source.right, source.top, source.bottom);
        }
    }
}
