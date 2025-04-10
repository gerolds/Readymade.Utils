#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using NaughtyAttributes;
#endif
using Readymade.Machinery.Acting;
using Readymade.Utils.Patterns;
using Readymade.Utils;
using UnityEngine;
using Vertx.Debugging;

namespace Readymade.Utils.Portals
{
    public class PortalExit : MonoBehaviour
    {
        [BoxGroup("System")] [SerializeField] private SoProp identity;
        [BoxGroup("FX")] [SerializeField] private AudioClip exitClip;
        private PortalSystem _system;

        public SoProp Identity => identity;
        public Pose ExitPose => new Pose(transform.position, transform.rotation);

        public AudioClip ExitClip => exitClip;

        private void Start()
        {
            if (!_system)
            {
                _system = Services.Get<PortalSystem>();
            }

            _system.Register(this);
        }

        private void OnDestroy()
        {
            if (_system)
            {
                _system.UnRegister(this);
            }
        }

        private void OnDrawGizmos()
        {
            D.raw(new Shape.Circle(transform.position, transform.up, .5f));
            D.raw(new Shape.Axis(ExitPose.position, ExitPose.rotation, false, Shape.Axes.All, 1f));
            D.raw(new Shape.Text(ExitPose.position, $"Exit\n{(identity ? identity.name : "No Identity")}"));
        }
    }
}