using System;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace AspectSwitcher
{
    [Serializable]
    public class ImageData : AspectSwitcher.ComponentSnapshotData<Image>
    {

        public Sprite sprite = null;
        public Color color = Color.white;
        public Material material = null;
        public bool raycastTarget = true;
        public UnityEngine.UI.Image.Type imageType = Image.Type.Simple;
        public float pixelsPerUnitMultiplier = 1f;
        
        public override void CaptureFrom(Component target)
        {
            if (target == null) return;
            var img = target.gameObject.GetComponent<Image>();
            if (img != null)
            {
                action = LayoutAction.Update;
                sprite = img.sprite;
                color = img.color;
                material = img.material;
                raycastTarget = img.raycastTarget;
                imageType = img.type;
                pixelsPerUnitMultiplier = img.pixelsPerUnitMultiplier;
            }
            else
            {
                action = LayoutAction.Remove;
            }
        }

        protected override void CreateComponent(Component target, SnapshotData previousStateData, float t)
        {
            var img = target.gameObject.AddComponent<Image>();
            UpdateComponent(target, previousStateData, t, img);
        }

        protected override void UpdateComponent(Component target, SnapshotData previousStateData, float t, Image img)
        {
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