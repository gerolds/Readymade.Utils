using System;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using NaughtyAttributes;
#endif
using UnityEngine;
using Vertx.Debugging;

namespace Readymade.Utils.Prototyping
{
    public class DrawShape : MonoBehaviour
    {
        [SerializeField] private ShapeSwitch _shape;

        [ShowIf(nameof(_shape), ShapeSwitch.Sphere)]
        [SerializeField] private float _radius = .3f;

        [ShowIf(nameof(_shape), ShapeSwitch.Box)]
        [SerializeField] private Vector3 _size = Vector3.one * .3f;

        [SerializeField] private Color _color = Color.white;

        private void OnDrawGizmos()
        {
            switch (_shape)
            {
                case ShapeSwitch.Point:
                    D.raw(new Shape.Point(transform.position), _color);
                    break;
                case ShapeSwitch.Sphere:
                    D.raw(new Shape.Sphere(transform.position, _radius), _color);
                    break;
                case ShapeSwitch.Box:
                    D.raw(new Shape.Box(transform.position, _size), _color);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    public enum ShapeSwitch
    {
        Point,
        Sphere,
        Box
    }
}