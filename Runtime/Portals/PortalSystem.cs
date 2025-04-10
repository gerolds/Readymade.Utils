using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Eflatun.SceneReference;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using NaughtyAttributes;
#endif
using Readymade.Persistence;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using Debug = UnityEngine.Debug;
using Progress = Pathfinding.Progress;

namespace Readymade.Utils.Portals
{
    public class PortalSystem : MonoBehaviour
    {
#if ODIN_INSPECTOR
        [ListDrawerSettings(ShowPaging = false, ShowFoldout = false)]
#else
        [ReorderableList]
#endif
        [SerializeField] private TransitConfig[] connectivity;

        [SerializeField] private GameObject whileInTransit;

        [SerializeField] private SimpleProgressDisplay progressDisplay;
        [SerializeField] private TransitScreenDisplay transitDisplay;
        [SerializeField] private float delayScreen = 1f;
        [SerializeField] private AstarPath pathfinding;

        [FormerlySerializedAs("audioSource")]
        [SerializeField]
        private AudioSource sfxAudioSource;

        [BoxGroup("Events")] [SerializeField] private bool saveOnTransit;
        [BoxGroup("Events")] [SerializeField] private UnityEvent onTransit;
        [BoxGroup("Events")] [SerializeField] private SceneIDUnityEvent onSceneLoaded;

        [SerializeField]
        [ShowIf(nameof(saveOnTransit))]
        [Required]
        private PackSystem packSystem;

        private readonly HashSet<PortalComponent> _portals = new();
        private readonly Dictionary<int, HashSet<PortalExit>> _exits = new();
        private readonly HashSet<PortalUser> _usersInTransit = new();
        [SerializeField] private bool debug;

        public event Action<PortalUser> Transitioning;
        public event Action<PortalUser> Transitioned;

        private void Start()
        {
            Debug.Assert(saveOnTransit && packSystem, "saveOnTransit && packSystem", this);
            whileInTransit?.SetActive(false);
            transitDisplay.fadeGroup.alpha = 0;
            transitDisplay.info.text = "No information available.";
        }

        public void Register(PortalComponent component)
        {
            _portals.Add(component);
            if (debug)
            {
                Debug.Log($"[{nameof(PortalSystem)}] Registered {nameof(PortalComponent)} {component.name}", this);
            }
        }

        public void UnRegister(PortalComponent component)
        {
            _portals.Remove(component);
            if (debug)
            {
                Debug.Log($"[{nameof(PortalSystem)}] Un-registered {nameof(PortalComponent)} {component.name}", this);
            }
        }

        public void Register(PortalExit component)
        {
            if (!_exits.ContainsKey(component.gameObject.scene.buildIndex))
            {
                _exits[component.gameObject.scene.buildIndex] = new HashSet<PortalExit>();
            }

            _exits[component.gameObject.scene.buildIndex].Add(component);
            if (debug)
            {
                Debug.Log($"[{nameof(PortalSystem)}] Registered {nameof(PortalExit)} {component.name}", this);
            }
        }

        public void UnRegister(PortalExit component)
        {
            _exits[component.gameObject.scene.buildIndex].Remove(component);
            if (debug)
            {
                Debug.Log($"[{nameof(PortalSystem)}] Un-registered {nameof(PortalExit)} {component.name}", this);
            }
        }
        
        public bool CanTransit { get; set; } = true;

        public async UniTask<bool> TryEnterAsync(PortalComponent portal, PortalUser user)
        {
            if (!CanTransit)
            {
                return false;
            }

            Debug.Log(
                $"[{nameof(PortalSystem)}] {nameof(PortalUser)} {user.name} has entered {nameof(PortalComponent)} {portal.name}.",
                this);

            // the portal will go out of scope on scene-load so keep a direct reference to the config object.
            TransitConfig config = portal.Config;

            portal.OnEnter(user);

            user.OnEnter(portal);

            if (portal.EnterClip)
            {
                PlaySfx(portal.EnterClip);
            }

            return await TryTransitAsync(config, user);
        }

        public async UniTask<bool> TryTransitAsync(TransitConfig config, PortalUser user)
        {
            if (!CanTransit)
            {
                return false;
            }

            try
            {
                Stopwatch sw = Stopwatch.StartNew();
                _usersInTransit.Add(user);

                Transitioning?.Invoke(user);

                transitDisplay.info.text = $"{config.transitInfo}";
                transitDisplay.fadeGroup.alpha = 0;
                await transitDisplay.fadeGroup.DOFade(1, .2f).AsyncWaitForCompletion();
                whileInTransit?.SetActive(true);
                onTransit?.Invoke();

                if (saveOnTransit)
                {
                    Debug.Log($"[{nameof(PortalSystem)}] Saving on transit", this);
                    await packSystem.SaveAsync();
                    Debug.Log($"[{nameof(PortalSystem)}] Saved on transit", this);
                }

                Progress<float> progress = new Progress<float>();
                // wait for a physics update to allow trigger events to fire.
                await UniTask.NextFrame(PlayerLoopTiming.FixedUpdate);

                progress.ProgressChanged += (_, t) => progressDisplay.Fill.fillAmount = t;

                // unload any scene that needs to be unloaded.
                foreach (var toUnload in config.Unload)
                {
                    if (toUnload.State == SceneReferenceState.Regular)
                    {
                        if (config.SceneMode == LoadSceneMode.Additive &&
                            toUnload.LoadedScene.IsValid()
                        )
                        {
                            AsyncOperation op1 = SceneManager.UnloadSceneAsync(toUnload.BuildIndex);
                            await op1.ToUniTask(progress, PlayerLoopTiming.Update, CancellationToken.None);
                            Debug.Log($"[{nameof(PortalSystem)}] Unloaded scene: {toUnload.Name}",
                                this);
                        }
                    }
                }

                // wait for a physics update to allow trigger events to fire.
                await UniTask.NextFrame(PlayerLoopTiming.FixedUpdate);

                // load the scene to transit to.
                if (config.Load.State == SceneReferenceState.Regular)
                {
                    if (!config.Load.LoadedScene.IsValid())
                    {
                        await LoadSceneAsync(config.Load.BuildIndex, LoadSceneMode.Additive, progress);

                        SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(config.ActivateScene.buildIndex));

                        // any exits that are part of the loaded scene will have registered themselves with the system so
                        // we can look them up by scene index.

                        // find exit spawner in loaded scene
                        bool noExit =
                            !_exits.TryGetValue(config.scene.BuildIndex,
                                out HashSet<PortalExit> exitCandidates) ||
                            exitCandidates.Count == 0;
                        if (noExit)
                        {
                            Debug.LogError($"[{nameof(PortalSystem)}] No exit found in scene {config.Load.Name}",
                                this);

                            FindLoadedExit(config);
                        }
                        else
                        {
                            PortalExit defaultExit = exitCandidates.First();
                            PortalExit exit = exitCandidates.FirstOrDefault(it => it.Identity == config.ExitIdentity);
                            if (exit == default)
                            {
                                Debug.LogError(
                                    $"[{nameof(PortalSystem)}] No exit found for '{config.ExitIdentity.name}', using default {defaultExit?.Identity}",
                                    this);
                                user.OnExit(defaultExit);
                                PlaySfx(defaultExit?.ExitClip);
                            }
                            else
                            {
                                if (debug)
                                {
                                    Debug.Log(
                                        $"[{nameof(PortalSystem)}] Exit found for '{config.ExitIdentity.name}' at {exit.ExitPose.position}.",
                                        this);
                                }

                                user.OnExit(exit);
                                PlaySfx(exit.ExitClip);
                            }
                        }
                    }
                    else
                    {
                        FindLoadedExit(config);
                    }
                }
                else
                {
                    FindLoadedExit(config);
                }

                progressDisplay.Fill.fillAmount = 1f;

                // wait a bit before releasing the screen.
                if (debug)
                {
                    Debug.Log(
                        $"[{nameof(PortalSystem)}] Transit of {nameof(PortalUser)} {user.name} completed ({sw.ElapsedMilliseconds} ms).",
                        this);
                }

                if (pathfinding)
                {
                    IEnumerable<Progress> progressRt = pathfinding.ScanAsync();

                    /*
                    foreach (var p in progressRt)
                    {
                        await UniTask.NextFrame();
                        progressDisplay.Fill.fillAmount = p.progress;
                    }

                    progressDisplay.Fill.fillAmount = 1f;
                    */
                }

                await UniTask.Delay(TimeSpan.FromSeconds(delayScreen));

                return true;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return false;
            }
            finally
            {
                Transitioned?.Invoke(user);
                whileInTransit?.SetActive(false);
                _usersInTransit.Remove(user);
                StopLoop();

                // wait a few frames to allow other systems to settle (e.g. cinemachine).
                await UniTask.Delay(TimeSpan.FromSeconds(delayScreen));
                await UniTask.NextFrame(PlayerLoopTiming.FixedUpdate);
                await UniTask.NextFrame(PlayerLoopTiming.PostLateUpdate);
                await UniTask.NextFrame(PlayerLoopTiming.PostLateUpdate);

                await transitDisplay.fadeGroup.DOFade(0, 1f).AsyncWaitForCompletion();
            }

            void FindLoadedExit(TransitConfig transitConfig)
            {
                // find exit spawner in loaded scenes

                HashSet<PortalExit> exitCandidates = _exits.Values
                    .SelectMany(it => it)
                    .ToHashSet();

                if (exitCandidates.Count == 0)
                {
                    Debug.LogError($"[{nameof(PortalSystem)}] No exit found in any loaded scene while processing {transitConfig.name} which loads '{transitConfig.scene?.Name}'.",
                        this);
                }
                else
                {
                    PortalExit defaultExit = exitCandidates.First();
                    PortalExit exit = exitCandidates.FirstOrDefault(it => it.Identity == transitConfig.ExitIdentity);
                    if (exit == default)
                    {
                        Debug.LogError(
                            $"[{nameof(PortalSystem)}] No exit found for {transitConfig.ExitIdentity}, using default {defaultExit?.Identity}",
                            this);
                        user.OnExit(defaultExit);
                    }
                    else
                    {
                        user.OnExit(exit);
                        PlaySfx(exit.ExitClip);
                    }
                }
            }
        }

        private async Task LoadSceneAsync(int buildIndex, LoadSceneMode mode, Progress<float> progress)
        {
            // load the scene only if it's not already loaded (we don't want duplicates).
            Stopwatch sw = Stopwatch.StartNew();
            AsyncOperation op = SceneManager.LoadSceneAsync(buildIndex, mode);
            await op.ToUniTask(progress, PlayerLoopTiming.Update, CancellationToken.None);
            Scene scene = SceneManager.GetSceneByBuildIndex(buildIndex);
            onSceneLoaded.Invoke(buildIndex);
            if (debug)
            {
                Debug.Log($"[{nameof(PortalSystem)}] Loaded scene: {scene.name} ({sw.ElapsedMilliseconds} ms)", this);
            }

            await UniTask.NextFrame();
        }

        private void PlaySfx(AudioClip clip) => sfxAudioSource?.PlayOneShot(clip);

        private void PlayLoop(AudioClip clip, float delay)
        {
            sfxAudioSource.clip = clip;
            sfxAudioSource.loop = true;
            sfxAudioSource.PlayDelayed(delay);
        }

        private void StopLoop()
        {
            sfxAudioSource.Stop();
        }

        
    }

    [Serializable]
    public class SceneIDUnityEvent : UnityEvent<int>
    {
    }

    [Serializable]
    public class SceneUnityEvent : UnityEvent<SceneReference>
    {
    }
}