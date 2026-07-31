using System;
using AspectSwitcher;
using UnityEngine;
using UnityEngine.UI;

namespace UnityEngine.UI.Snapshots
{
    [Serializable]
    public class GridLayoutSnapshotData : AspectSwitcher.ComponentSnapshotData<GridLayoutGroup>
    {

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
                action = LayoutAction.Update;
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

        protected override void CreateComponent(Component target, SnapshotData previousStateData, float t)
        {
            if (t < 1f) return;
            var grid = target.gameObject.AddComponent<GridLayoutGroup>();
            UpdateComponent(target, previousStateData, t, grid);
        }

        protected override void UpdateComponent(Component target, SnapshotData previousStateData, float t, GridLayoutGroup component)
        {
            if (t < 1f) return;
            component.cellSize = cellSize;
            component.spacing = spacing;
            component.padding = CopyPadding(padding);
            component.startCorner = startCorner;
            component.startAxis = startAxis;
            component.childAlignment = childAlignment;
            component.constraint = constraint;
            component.constraintCount = Mathf.Max(1, constraintCount);
        }

        private static RectOffset CopyPadding(RectOffset source)
        {
            return source == null
                ? new RectOffset()
                : new RectOffset(source.left, source.right, source.top, source.bottom);
        }
    }
}
