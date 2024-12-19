using System;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;

namespace Readymade.Utils
{
    [Serializable]
    public class ObjectUnityEvent : UnityEvent<Object>
    {
    }
}