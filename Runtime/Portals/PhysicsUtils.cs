using Unity.Mathematics;
using UnityEngine;

namespace Readymade.Utils.Portals
{
    public static class PhysicsUtils
    {
        public static bool OverlapPoint(this BoxCollider collider, Vector3 point)
            => OverlapBoxPoint( collider.size * 0.5f, collider.transform.InverseTransformPoint(point) - collider.center);

        public static bool OverlapPoint(this SphereCollider collider, float3 point)
            => OverlapSpherePoint(collider.radius, collider.transform.InverseTransformPoint(point) - collider.center);

        public static bool OverlapBoxPoint(float3 size, float3 point)
            => BoxSDF(point, size) < 0;

        public static bool OverlapSpherePoint(float r, float3 point)
            => SphereSDF(point, r) < 0;

        public static float CapsuleSDF(float3 point, float3 a, float3 b, float r)
        {
            float3 pa = point - a;
            float3 ba = b - a;
            float ratio = math.dot(pa, ba) / math.dot(ba, ba);
            float h = math.clamp(ratio, 0.0f, 1.0f);
            return math.length(pa - ba * h) - r;
        }

        public static float BoxSDF(float3 point, float3 size)
        {
            float3 q = new float3(math.abs(point.x), math.abs(point.y), math.abs(point.z)) - size;
            return math.length(math.max(q, float3.zero)) + math.min(math.max(q.x, math.max(q.y, q.z)), 0.0f);
        }

        public static float SphereSDF(float3 point, float r)
        {
            return math.lengthsq(point) - r * r;
        }
    }
}