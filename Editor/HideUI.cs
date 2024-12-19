using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Readymade.Utils.Editor
{
    public static class HideUI
    {
        private static bool _isOn = true;

        [MenuItem("Tools/UI/Enable Overlay Canvases")]
        [InitializeOnEnterPlayMode]
        public static void EnableOverlayCanvasUI() => SetOverlayCanvasUI(true);

        [MenuItem("Tools/UI/Disable Overlay Canvases")]
        public static void DisableOverlayCanvasUI() => SetOverlayCanvasUI(false);

        public static void SetOverlayCanvasUI(bool isOn)
        {
            _isOn = isOn;
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (scene.isLoaded == false) continue;
                foreach (var go in scene.GetRootGameObjects())
                {
                    Canvas[] canvases = go.GetComponentsInChildren<Canvas>(true);
                    foreach (var canvas in canvases)
                    {
                        if (canvas &&
                            canvas.isRootCanvas &&
                            canvas.renderMode == RenderMode.ScreenSpaceOverlay
                        )
                        {
                            canvas.gameObject.SetActive(_isOn);
                            Debug.Log($"{nameof(Canvas)} {canvas.name} {(_isOn ? "enabled" : "disabled")}.");
                        }
                    }
                }
            }
        }
    }
}