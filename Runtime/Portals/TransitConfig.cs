using System;
using Eflatun.SceneReference;
using Readymade.Machinery.Acting;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace Readymade.Utils.Portals
{
    [CreateAssetMenu(menuName = "App/Core/Portals/PortalConfig", fileName = "PortalConfig")]
    public class TransitConfig : ScriptableObject
    {
        [NaughtyAttributes.BoxGroup("Destination")]
        [SerializeField]
        private bool loadScene;

        [NaughtyAttributes.BoxGroup("Destination")]
        [SerializeField]
        [NaughtyAttributes.ShowIf(nameof(loadScene))]
        private LoadSceneMode loadSceneMode = LoadSceneMode.Additive;

        [NaughtyAttributes.BoxGroup("Destination")]
        [SerializeField]
        [NaughtyAttributes.ShowIf(nameof(loadScene))]
        public SceneReference scene = new();

        [NaughtyAttributes.BoxGroup("Active Scene")]
        [SerializeField]
        [NaughtyAttributes.ShowIf(nameof(ShowActiveMode))]
        private Activate activateScene = Activate.Default;

        [NaughtyAttributes.BoxGroup("Active Scene")]
        [SerializeField]
        [NaughtyAttributes.ShowIf(nameof(ShowActiveScene))]
        public SceneReference activeScene = new();

        private bool ShowActiveMode => loadScene &&
            loadSceneMode == LoadSceneMode.Additive;
        bool ShowActiveScene => loadScene &&
            loadSceneMode == LoadSceneMode.Additive &&
            activateScene == Activate.OtherScene;

        [FormerlySerializedAs("sceneToUnload")]
        [NaughtyAttributes.BoxGroup("Side Effects")]
        [NaughtyAttributes.ShowIf(nameof(loadSceneMode), LoadSceneMode.Additive)]
        [SerializeField]
#if ODIN_INSPECTOR
        [ListDrawerSettings(ShowPaging = false, ShowFoldout = false)]
#else
        [ReorderableList]
#endif
        private SceneReference[] unload;

        [FormerlySerializedAs("spawnerIdentity")]
        [NaughtyAttributes.BoxGroup("Destination")]
        [SerializeField]
        private SoProp exitIdentity;

        [Tooltip("Description to display while in transit.")]
        [SerializeField]
        public string transitInfo = "Undefined";

        public bool LoadScene => loadScene;

        public SceneReference Load => scene;

        public SceneReference[] Unload => unload;

        public SoProp ExitIdentity => exitIdentity;

        public LoadSceneMode SceneMode => loadSceneMode;

        public Scene ActivateScene => activateScene switch
        {
            Activate.Default => SceneManager.GetActiveScene(),
            Activate.LoadedScene => loadScene
                ? scene.LoadedScene
                : SceneManager.GetActiveScene(),
            Activate.OtherScene => activeScene.LoadedScene.isLoaded
                ? activeScene.LoadedScene
                : SceneManager.GetActiveScene(),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public enum Activate
    {
        Default,
        LoadedScene,
        OtherScene,
    }
}