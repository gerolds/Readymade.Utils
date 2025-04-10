using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using Eflatun.SceneReference;
using JBooth.MicroSplat;
using JBooth.MicroVerseCore;
using Readymade.Utils.Patterns;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;
using Vertx.Debugging;

namespace App.Core.Streaming
{
    public class StreamingGroup : MonoBehaviour
    {
        [SerializeField] private float chunkSize = 500;

        [SerializeField] private string token = "default";

        [SerializeField] private List<SceneReference> scenes;

        [SerializeField] private List<GameObject> surrogates;

        [SerializeField] private List<Texture2D> globalTxTiles;

        [SerializeField] private Vector2Int chunkCount = new(5, 5);

        [SerializeField] private GameObject chunkPrefab;

        [SerializeField] private Transform microVerse;

        [Min(0.75f)]
        [MaxValue(3)]
        [SerializeField]
        [Tooltip("The distance from the trigger at which to load a chunk. The value is multiplied by the chunk size.")]
        private float loadNeighbours = 1.5f;

        [Min(0)]
        [Tooltip(
            "A delay in seconds before loading the chunks. This can be useful to prevent loading chunks when the " +
            "player is only briefly entering the threshold distance.")]
        [SerializeField]
        private float delayLoad = 1f;

        private StreamingSystem _sys;
        private Vector3 _prevPos;
        private Vector3 _pos;
        private HashSet<int> _loading = new();
        private Dictionary<int, float> _loadTimers = new();

        [ShowInInspector] public bool IsUpdating => _loading.Count > 0;
        public IEnumerable<SceneReference> Scenes => scenes;

        public void SetActive(bool isActive) => enabled = isActive;

        private void Start()
        {
            _sys = Services.Get<StreamingSystem>();
            _sys.Register(this);
        }

        private void OnDestroy()
        {
            _sys?.UnRegister(this);
        }

        public void Tick(Vector3 triggerPosition)
        {
            // don't update while scenes are (un-)loading.
            if (_loading.Count > 0 || SceneManager.sceneCount != SceneManager.loadedSceneCount)
            {
                return;
            }

            if (Vector3.Distance(_prevPos, triggerPosition) < chunkSize / 64f)
            {
                return;
            }

            UpdateAsync(triggerPosition).Forget();
        }

        private async UniTaskVoid UpdateAsync(Vector3 triggerPosition)
        {
            _prevPos = triggerPosition;
            for (int x = 0; x < chunkCount.x; x++)
            {
                for (int y = 0; y < chunkCount.y; y++)
                {
                    int ndx = x * chunkCount.y + y;
                    if (ndx < 0 || ndx >= scenes.Count)
                    {
                        continue;
                    }

                    if (!CheckTimer(ndx))
                    {
                        continue;
                    }

                    if (ShouldLoad(x, y, triggerPosition))
                    {
                        if (!scenes[ndx].LoadedScene.isLoaded)
                        {
                            ResetTimer(ndx);
                            Debug.Assert(!SceneManager.GetSceneByBuildIndex(scenes[ndx].BuildIndex).isLoaded,
                                "ASSERTION FAILED: IsNotLoaded", this);
                            Debug.Log(
                                $"[{nameof(StreamingSystem)}] Loading chunk ({x}, {y}) at #{ndx}, {scenes[ndx].Name}");
                            _loading.Add(scenes[ndx].BuildIndex);
                            try
                            {
                                await SceneManager.LoadSceneAsync(scenes[ndx].BuildIndex, LoadSceneMode.Additive);
                                surrogates[ndx].SetActive(false);
                            }
                            catch (Exception e)
                            {
                                Debug.LogException(e);
                            }
                            finally
                            {
                                _loading.Remove(scenes[ndx].BuildIndex);
                            }
                        }
                    }
                    else
                    {
                        if (scenes[ndx].LoadedScene != default)
                        {
                            ResetTimer(ndx);
                            Debug.Log(
                                $"[{nameof(StreamingSystem)}] Un-loading chunk ({x}, {y}) at #{ndx}, {scenes[ndx].Name}");
                            _loading.Add(scenes[ndx].BuildIndex);
                            try
                            {
                                await SceneManager.UnloadSceneAsync(scenes[ndx].LoadedScene);
                                surrogates[ndx].SetActive(true);
                            }
                            catch (Exception e)
                            {
                                Debug.LogException(e);
                            }
                            finally
                            {
                                _loading.Remove(scenes[ndx].BuildIndex);
                            }
                        }
                    }
                }
            }
        }

        private void ResetTimer(int index) => _loadTimers[index] = Time.time + delayLoad;

        private bool CheckTimer(int index) => !_loadTimers.TryGetValue(index, out float timer) || timer < Time.time;

        private bool ShouldLoad(int x, int y, Vector3 pos)
        {
            var triggerPos = pos;
            var bounds = GetBounds(x, y, triggerPos);

            Vector3 closestPoint = bounds.ClosestPoint(triggerPos);
            bool shouldLoad = Vector3.Distance(closestPoint, triggerPos) < loadNeighbours * chunkSize;
            return shouldLoad;
        }

        private Bounds GetBounds(int x, int y, Vector3 observer)
        {
            Vector3 offset = new Vector3(chunkSize * chunkCount.x / 2 - chunkSize / 2f, 0,
                chunkSize * chunkCount.y / 2 - chunkSize / 2f);
            Bounds bounds = new Bounds(
                new Vector3(chunkSize * x, observer.y, chunkSize * y) - offset,
                Vector3.one * chunkSize
            );
            return bounds;
        }

        private Bounds GetBounds(Vector3 pos)
        {
            Vector2Int cell = GetCell(pos);
            Vector3 offset = new Vector3(chunkSize * chunkCount.x / 2, 0, chunkSize * chunkCount.y / 2);
            Bounds bounds = new Bounds(
                new Vector3(chunkSize * cell.x, 0, chunkSize * cell.y) - offset,
                Vector3.one * chunkSize
            );
            return bounds;
        }

        public int GetIndex(Vector3 pos)
        {
            Vector2Int cell = GetCell(pos);
            int ndx = cell.x * chunkCount.y + cell.y;
            return ndx;
        }

        private Vector2Int GetCell(Vector3 pos)
        {
            Vector3 offset = new Vector3(chunkSize * chunkCount.x / 2, 0, chunkSize * chunkCount.y / 2);
            Vector3 oPos = pos + offset;
            int x = Mathf.FloorToInt(oPos.x / chunkSize);
            int y = Mathf.FloorToInt(oPos.z / chunkSize);
            return new Vector2Int(x, y);
        }

        private void OnDrawGizmosSelected()
        {
            Vector3 pos = _prevPos;

            D.raw(pos, new Color(0, 0.5f, 1.0f, 1.0f));
            D.raw(new Shape.Sphere(pos, chunkSize * loadNeighbours), new Color(0, 0.5f, 1.0f, 1.0f));
            for (int x = 0; x < chunkCount.x; x++)
            {
                for (int y = 0; y < chunkCount.y; y++)
                {
                    int ndx = x * chunkCount.y + y;
                    if (ndx < 0 || ndx >= scenes.Count)
                    {
                        continue;
                    }


                    Bounds bounds = GetBounds(x, y, Vector3.zero);
                    var b1 = new Bounds(bounds.center, bounds.size * 0.95f);
                    var b2 = new Bounds(bounds.center, bounds.size * 0.90f);

                    if (scenes[ndx].LoadedScene.isLoaded)
                    {
                        D.raw(new Shape.Text(b1.center, $"#{ndx} x{x} y{y}"), new Color(0, 0.5f, 1.0f, 1.0f),
                            Color.white);
                        D.raw(new Shape.Box(b1, false), new Color(0, 0.5f, 1.0f, 1.0f));
                    }
                    else
                    {
                        D.raw(new Shape.Text(b1.center, $"#{ndx} x{x} y{y}"), Color.gray, Color.white);
                        D.raw(new Shape.Box(b1, false), Color.gray);
                    }

                    if (ShouldLoad(x, y, _prevPos))
                    {
                        D.raw(new Shape.Box(b2, false), Color.yellow);
                    }
                }
            }
        }

        [Button]
        private void OpenAllScenes()
        {
            foreach (SceneReference scene in scenes)
            {
                if (!scene.LoadedScene.isLoaded)
                {
                    UnityEditor.SceneManagement.EditorSceneManager.OpenScene(scene.Path,
                        UnityEditor.SceneManagement.OpenSceneMode.Additive);
                }
            }

            foreach (var surrogate in surrogates)
            {
                surrogate.SetActive(false);
            }
        }

        [Button]
        private void CloseAllScenes()
        {
            foreach (SceneReference scene in scenes)
            {
                if (scene.LoadedScene.isLoaded)
                {
                    UnityEditor.SceneManagement.EditorSceneManager.CloseScene(scene.LoadedScene, true);
                }
            }

            foreach (var surrogate in surrogates)
            {
                surrogate.SetActive(true);
            }
        }

        [Button]
        private void CollectObjectsFromScenes()
        {
#if UNITY_EDITOR
            microVerse.transform.position = Vector3.zero;
            microVerse.transform.rotation = Quaternion.identity;
            microVerse.transform.localScale = Vector3.one;
            UnityEditor.EditorUtility.SetDirty(microVerse);
            foreach (var scene in scenes)
            {
                foreach (var rootGameObject in scene.LoadedScene.GetRootGameObjects())
                {
                    SceneManager.MoveGameObjectToScene(rootGameObject, microVerse.gameObject.scene);
                    UnityEditor.EditorUtility.SetDirty(rootGameObject);
                    rootGameObject.transform.SetParent(microVerse, true);
                }
            }
#endif
        }

        [Button]
        private void ApplyGlobalTextures()
        {
            for (var i = 0; i < scenes.Count; i++)
            {
                Debug.Log($"Processing scene {i}");
                SceneReference scene = scenes[i];
                if (!scene.LoadedScene.isLoaded)
                {
                    UnityEditor.SceneManagement.EditorSceneManager.OpenScene(scene.Path,
                        UnityEditor.SceneManagement.OpenSceneMode.Additive);
                }

                var t = scene.LoadedScene.GetRootGameObjects()
                    .Where(it => it.TryGetComponent(out MicroSplatTerrain _))
                    .Select(it => it.GetComponent<MicroSplatTerrain>())
                    .FirstOrDefault();
                if (t)
                {
                    t.tintMapOverride = globalTxTiles[i];
                    UnityEditor.EditorUtility.SetDirty(t);
                }
            }
        }

        [Button]
        private void MoveObjectsIntoScenes()
        {
#if UNITY_EDITOR
            var toMove = new List<GameObject>();
            for (int i = microVerse.childCount - 1; i >= 0; i--)
            {
                toMove.Add(microVerse.GetChild(i).gameObject);
            }

            var distinctObjects = toMove
                .Distinct()
                .OrderBy(it => GetIndex(it.transform.position))
                .ToList();

            foreach (GameObject o in distinctObjects)
            {
                int sceneIndex = GetIndex(o.transform.position);
                if (sceneIndex >= 0 && sceneIndex < scenes.Count)
                {
                    if (!scenes[sceneIndex].LoadedScene.isLoaded)
                    {
                        UnityEditor.SceneManagement.EditorSceneManager.OpenScene(scenes[sceneIndex].Path,
                            UnityEditor.SceneManagement.OpenSceneMode.Additive);
                    }

                    o.transform.SetParent(null, true);
                    SceneManager.MoveGameObjectToScene(o, scenes[sceneIndex].LoadedScene);
                    UnityEditor.EditorUtility.SetDirty(o);
                }
                else
                {
                    Debug.Log($"Object '{o.name}' at {o.transform.position} is out of bounds ({sceneIndex})", o);
                }
            }
#endif
        }

        [Button]
        private void GenerateScenes()
        {
#if UNITY_EDITOR
            scenes.Clear();
            for (int x = 0; x < chunkCount.x; x++)
            {
                for (int y = 0; y < chunkCount.y; y++)
                {
                    string sceneName = $"Chunk_{x}_{y}";
                    string saveDirPath = Path.Combine("Assets", "Scenes", token);
                    string saveFilePath = Path.Combine(saveDirPath, $"{sceneName}.unity");
                    Vector3 chunkCenter = new Vector3(x * chunkSize, 0, y * chunkSize);
                    Directory.CreateDirectory(saveDirPath);
                    if (!File.Exists(saveFilePath))
                    {
                        Scene scene = UnityEditor.SceneManagement.EditorSceneManager.NewScene(
                            UnityEditor.SceneManagement.NewSceneSetup.EmptyScene,
                            UnityEditor.SceneManagement.NewSceneMode.Additive);
                        SceneManager.SetActiveScene(scene);
                        GameObject n = new GameObject(sceneName, typeof(BoxCollider));
                        BoxCollider c = n.GetComponent<BoxCollider>();
                        c.size = new Vector3(chunkSize, chunkSize, chunkSize);
                        n.transform.position = chunkCenter;
                    }

                    UnityEditor.GUID guid = UnityEditor.AssetDatabase.GUIDFromAssetPath(saveFilePath);
                    SceneReference sceneRef = new SceneReference(guid.ToString());
                    scenes.Add(sceneRef);
                    if (sceneRef.LoadedScene == default)
                    {
                        UnityEditor.SceneManagement.EditorSceneManager.LoadScene(sceneRef.BuildIndex);
                    }

                    GameObject prefabInstance = Instantiate(chunkPrefab);
                    prefabInstance.transform.position = chunkCenter;
                    UnityEditor.SceneManagement.EditorSceneManager.SaveScene(sceneRef.LoadedScene, saveFilePath);
                }
            }

            scenes = scenes.OrderBy(it => it.Name).ToList();
#endif
        }
    }
}