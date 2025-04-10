using System;
using System.Collections.Generic;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using NaughtyAttributes;
#endif
using Pathfinding.Drawing;
using Readymade.Machinery.Shared.PriorityQueue;
using UnityEngine;

namespace Readymade.Utils.Portals
{
    [ExecuteAlways]
    public class RenderVolumeSystem : MonoBehaviour
    {
        private SimplePriorityQueue<RenderVolume> _volumeStack = new();
        private HashSet<RenderVolume> _volumeStackSet = new();
        private HashSet<RenderVolume> _volumes = new();

        [SerializeField] [Required] private RenderVolumeProfile fallbackProfile;

        [SerializeField] private Transform trigger;

        private RenderVolumeProfile _activeProfile;
        private static RenderVolumeSystem _instance;

#if ODIN_INSPECTOR
        [ReadOnly]
        [ShowInInspector]
#else
        [ShowNativeProperty]
#endif
        public RenderVolumeProfile ActiveProfile => _activeProfile;
        
#if ODIN_INSPECTOR
        [ReadOnly]
        [ShowInInspector]
#else
        [ShowNativeProperty]
#endif
        public int LoadedVolumes => _volumes.Count;
        
#if ODIN_INSPECTOR
        [ReadOnly]
        [ShowInInspector]
#else
        [ShowNativeProperty]
#endif
        public int ActiveVolumes => _volumeStack.Count;

#if ODIN_INSPECTOR
        [ReadOnly]
        [ShowInInspector]
#else
        [ShowNativeProperty]
#endif
        public Vector3 TriggerPosition => Application.isPlaying
            ? trigger.transform.position
            :
#if UNITY_EDITOR
            UnityEditor.SceneView.lastActiveSceneView?.camera.transform.position ??
#endif
            trigger.transform.position;

        private void Awake()
        {
            // ensure singleton
            if (_instance != null && _instance != this)
            {
                Debug.LogWarning("There should only be one RenderVolumeSystem in the scene. Destroying the duplicate.");
                Destroy(this);
            }
            else
            {
                _instance = this;
            }
        }

        private void Start()
        {
            if (!trigger)
            {
                trigger = Camera.main?.transform;
            }
        }

        public void Register(RenderVolume component)
        {
            if (_volumes.Add(component))
            {
                Debug.Log($"Volume {component?.name} registered.", component);
            }
        }

        public void UnRegister(RenderVolume component)
        {
            Debug.Log($"Volume {component?.name} un-registered.", component);
            if (_volumes.Remove(component))
            {
                _volumeStackSet.Remove(component);
                if (_volumeStack.Contains(component))
                {
                    _volumeStack.Remove(component);
                }

                UpdateRenderSettings();
            }
        }

        public void OnEnter(RenderVolume renderVolume)
        {
            if (renderVolume)
            {
                _volumeStack.EnqueueWithoutDuplicates(renderVolume, renderVolume.Profile.Priority);
                UpdateRenderSettings();
            }
        }

        public void OnExit(RenderVolume renderVolume)
        {
            _volumeStack.Remove(renderVolume);
            UpdateRenderSettings();
        }

        [Button]
        private void Update()
        {
            // this is used only for updating the editor.

            if (Application.isPlaying || !trigger)
            {
                return;
            }

            Vector3 triggerPos = TriggerPosition;
            foreach (var volume in _volumes)
            {
                bool isInside = volume.Collider switch
                {
                    BoxCollider c => c.OverlapPoint(triggerPos),
                    SphereCollider s => s.OverlapPoint(triggerPos),
                    _ => throw new NotImplementedException()
                };

                if (isInside)
                {
                    if (_volumeStackSet.Add(volume))
                    {
                        OnEnter(volume);
                    }
                }
                else
                {
                    if (_volumeStackSet.Remove(volume))
                    {
                        OnExit(volume);
                    }
                }
            }
        }

        private void UpdateRenderSettings()
        {
            RenderVolumeProfile profile;
            if (_volumeStack.Count == 0 || !_volumeStack.First || !_volumeStack.First.Profile)
            {
                profile = fallbackProfile;
            }
            else
            {
                profile = _volumeStack.First.Profile;
            }

            _activeProfile = profile;
            RenderSettings.fog = profile.Fog;
            RenderSettings.fogEndDistance = profile.FogEndDistance;
            RenderSettings.fogStartDistance = profile.FogStartDistance;
            RenderSettings.fogMode = profile.FogMode;
            RenderSettings.fogColor = profile.FogColor;
            RenderSettings.fogDensity = profile.FogDensity;
            RenderSettings.ambientMode = profile.AmbientMode;
            RenderSettings.ambientSkyColor = profile.AmbientSkyColor;
            RenderSettings.ambientEquatorColor = profile.AmbientEquatorColor;
            RenderSettings.ambientGroundColor = profile.AmbientGroundColor;
            RenderSettings.ambientLight = profile.AmbientLight;
            RenderSettings.ambientIntensity = profile.AmbientIntensity;
            RenderSettings.defaultReflectionMode = profile.DefaultReflectionMode;
            RenderSettings.reflectionIntensity = profile.ReflectionIntensity;
            RenderSettings.subtractiveShadowColor = profile.SubtractiveShadowColor;
            RenderSettings.haloStrength = profile.HaloStrength;
            RenderSettings.flareFadeSpeed = profile.FlareFadeSpeed;
            RenderSettings.flareStrength = profile.FlareStrength;
            RenderSettings.skybox = profile.Skybox;
            Debug.Log("[RenderVolumeSystem] Render settings updated", this);
        }
    }
}