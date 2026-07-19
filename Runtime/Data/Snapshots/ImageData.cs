using System;
using UnityEngine;
using UnityEngine.UI;

namespace AspectSwitcher
{
    [Serializable]
    public class ImageData : SnapshotData
    {
        public Sprite sprite = null;
        public Color color = Color.white;
        public Material material = null;
        public bool raycastTarget = true;
        public UnityEngine.UI.Image.Type imageType = Image.Type.Simple;
        public float pixelsPerUnitMultiplier = 1f;
        public override void CaptureFrom(Component target)
        {
            if (!(target is Image img)) return;
            sprite = img.sprite;
            color = img.color;
            material = img.material;
            raycastTarget = img.raycastTarget;
            imageType = img.type;
            pixelsPerUnitMultiplier = img.pixelsPerUnitMultiplier;
        }

        public override void ApplyTo(Component target, SnapshotData previousStateData, float t)
        {
            if (!(target is Image img)) return;
            var previousImgData = previousStateData as ImageData;
            
            if (previousImgData == null || t >= 1f)
            {
                img.sprite = sprite;
                img.color = color;
                img.material = material;
                img.raycastTarget = raycastTarget;
                img.type = imageType;
                img.pixelsPerUnitMultiplier = pixelsPerUnitMultiplier;
                return;
            }
            
            img.pixelsPerUnitMultiplier = Mathf.Lerp(previousImgData.pixelsPerUnitMultiplier, pixelsPerUnitMultiplier, t);
            img.color = Color.Lerp(previousImgData.color, color, t);
        }
    }
}