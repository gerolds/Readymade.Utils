using System;
using Readymade.Utils.Patterns;
using UnityEngine;

namespace App.Core.Streaming
{
    public class StreamingUser : MonoBehaviour
    {
        private StreamingSystem _sys;

        [SerializeField] private int priority;

        public int Priority => priority;

        private void Start()
        {
            _sys = Services.Get<StreamingSystem>();
            _sys.Register(this);
        }

        private void OnDestroy()
        {
            _sys?.UnRegister(this);
        }
    }
}