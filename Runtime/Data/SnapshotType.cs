using UnityEngine;

namespace AspectSwitcher
{
    public enum SnapshotType { UITransform, Transform, ComponentField, AnimatorParam, CanvasGroup }

    public static class SnapshotTypeExtensions
    {
        public static ISnapshotData CreateData(this SnapshotType type) => type switch
        {
            SnapshotType.UITransform    => (ISnapshotData)new UITransformData(),
            SnapshotType.Transform      => new TransformData(),
            SnapshotType.ComponentField => new ComponentFieldData(),
            SnapshotType.AnimatorParam  => new AnimatorParamData(),
            SnapshotType.CanvasGroup    => new CanvasGroupData(),
            _                           => null
        };

        public static Component GetDefaultTarget(this SnapshotType type, GameObject go) => type switch
        {
            SnapshotType.UITransform   => (Component)go.GetComponent<RectTransform>(),
            SnapshotType.Transform     => go.transform,
            SnapshotType.AnimatorParam => go.GetComponent<Animator>(),
            SnapshotType.CanvasGroup   => go.GetComponent<CanvasGroup>(),
            _                          => null
        };
    }
}
