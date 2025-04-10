using System.Collections.Generic;
using System.Linq;
using Readymade.Machinery.Shared.PriorityQueue;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using NaughtyAttributes;
#endif
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace App.Core.Streaming
{
    public class StreamingSystem : MonoBehaviour
    {
        [FormerlySerializedAs("target")]
        [SerializeField]
        private Transform trigger;

        private Vector3 _prevPos;
        private Vector3 _pos;
        private HashSet<int> _loading = new();
        private Dictionary<int, float> _loadTimers = new();
        private SimplePriorityQueue<Transform, float> _trigger = new();
        private HashSet<StreamingGroup> _groups = new();

        [ShowInInspector] public bool IsUpdating => _loading.Count > 0;

        public void SetActive(bool isActive) => enabled = isActive;

        private void Start()
        {
            if (trigger)
            {
                _trigger.EnqueueWithoutDuplicates(trigger, 0);
            }
        }

        private void Update()
        {
            foreach (var group in _groups)
            {
                group.Tick(_trigger.First.position);
            }
        }

        public void Register(StreamingUser user)
        {
            _trigger.EnqueueWithoutDuplicates(user.transform, user.Priority);
            Debug.Log($"Streaming trigger changed to {_trigger.First.name}", user);
        }

        public void UnRegister(StreamingUser user)
        {
            _trigger.Remove(user.transform);
            if (_trigger.Count > 0)
            {
                Debug.Log($"Streaming trigger changed to {_trigger.First.name}", user);
            }
            else
            {
                Debug.Log($"Last streaming trigger removed");
            }
        }

        public void Register(StreamingGroup group)
        {
            _groups.Add(group);
        }

        public void UnRegister(StreamingGroup group)
        {
            _groups.Remove(group);
            foreach (var scene in group.Scenes.ToList())
            {
                if (scene.LoadedScene.isLoaded)
                {
                    SceneManager.UnloadSceneAsync(scene.LoadedScene);
                }
            }
        }
    }
}