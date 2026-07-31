using System;
using AspectSwitcher;
using UnityEngine;
using UnityEngine.UI;

namespace UnityEngine.UI.Snapshots
{
    [Serializable]
    public class GridLayoutSnapshotData : SnapshotData
    {
        public LayoutAction action = LayoutAction.Update;

        [Header("Grid Settings")]
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
            if (target == null) return;

            var grid = target.gameObject.GetComponent<GridLayoutGroup>();
            if (grid != null)
            {
                action = LayoutAction.Create;
                cellSize = grid.cellSize;
                spacing = grid.spacing;
                padding = CopyPadding(grid.padding);
                startCorner = grid.startCorner;
                startAxis = grid.startAxis;
                childAlignment = grid.childAlignment;
                constraint = grid.constraint;
                constraintCount = grid.constraintCount;
            }
            else
            {
                action = LayoutAction.Remove;
            }
        }

        public override void ApplyTo(Component target, SnapshotData previousStateData, float t)
        {
            if (target == null) return;
            if (t < 1f) return;

            var go = target.gameObject;

            if (action == LayoutAction.Remove)
            {
                var existing = go.GetComponent<GridLayoutGroup>();
                if (existing != null)
                    Object.DestroyImmediate(existing);
                return;
            }

            var grid = go.GetComponent<GridLayoutGroup>();

            if (grid == null && action != LayoutAction.Update)
                grid = go.AddComponent<GridLayoutGroup>();

            if (grid == null) return;

            grid.cellSize = cellSize;
            grid.spacing = spacing;
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
