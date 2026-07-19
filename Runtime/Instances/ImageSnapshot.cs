using System;
using UnityEngine;
using UnityEngine.UI;

namespace AspectSwitcher
{
    [Serializable]
    public class ImageSnapshotEntry : SnapshotEntry<ImageData> { }
    public class ImageSnapshot : AspectSnapshot<ImageData, ImageSnapshotEntry>
    {
        protected override Component FindDefaultTarget() => GetComponent<Image>();
    }
}