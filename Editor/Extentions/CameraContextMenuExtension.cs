#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace AspectSwitcher.Extentions
{
    public static class CameraContextMenuExtension
    {
        [MenuItem("CONTEXT/Camera/Assign to AspectRatioMonitor")]
        private static void PassCameraToStatic(MenuCommand command)
        {
            Camera targetCamera = (Camera)command.context;

            if (targetCamera != null)
            {
                AspectRatioMonitor.Camera = targetCamera;

                Debug.Log($"[AspectRatioSwitcher] Camera '{targetCamera.name}' assigned.");
            }
        }
    }
}
#endif
