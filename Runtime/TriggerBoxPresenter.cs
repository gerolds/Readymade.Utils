using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Readymade.Utils
{
    [RequireComponent(typeof(TriggerBox))]
    public class TriggerBoxPresenter : MonoBehaviour
    {
        private TriggerBox _box;

        [SerializeField] private GameObject whileAny;
        [SerializeField] private GameObject whileNone;

        private void Awake()
        {
            _box = GetComponent<TriggerBox>();
            Debug.Assert(_box, "No TriggerBox found on GameObject", this);
        }

        private void OnEnable()
        {
            _box.Any += StateChangedHandler;
            _box.None += StateChangedHandler;
        }

        private void StateChangedHandler(TriggerBox triggerBox, Object obj)
        {
            if (whileAny)
                whileAny.SetActive(_box.HasContact);

            if (whileNone)
                whileNone.SetActive(!_box.HasContact);
        }
    }
}