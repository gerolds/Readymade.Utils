using UnityEngine;

namespace App.Core.Utils
{
    public static class VectorExtensions
    {
        public static Vector3 Swizzle(this Vector3 v, int x, int y, int z) => new(v[x], v[y], v[z]);
        public static Vector3 Swizzle(this Vector3 v, float x, int y, int z) => new(x, v[y], v[z]);
        public static Vector3 Swizzle(this Vector3 v, int x, float y, int z) => new(v[x], y, v[z]);
        public static Vector3 Swizzle(this Vector3 v, int x, int y, float z) => new(v[x], v[y], z);
        public static Vector3 Swizzle(this Vector3 v, float x, float y, int z) => new(x, y, v[z]);
        public static Vector3 Swizzle(this Vector3 v, int x, float y, float z) => new(v[x], y, z);
        public static Vector3 Swizzle(this Vector3 v, float x, float y, float z) => new(x, y, z);

        public static Vector3 ToXZ(this Vector2 v) => new(v.x, 0, v.y);
        public static Vector3 ToXY(this Vector2 v) => new(v.x, v.y, 0);
        public static Vector3 ToYZ(this Vector2 v) => new(0, v.x, v.y);

        public static Vector3 Hadamard(this Vector3 a, Vector3 b) => new(a.x * b.x, a.y * b.y, a.z * b.z);
        public static Vector3 Hadamard(this Vector3 a, float x, float y, float z) => a.Hadamard(new Vector3(x, y, z));

        public static float Max(this Vector3 v) => Mathf.Max(Mathf.Max(v.x, v.y), v.z);
        public static float Min(this Vector3 v) => Mathf.Min(Mathf.Min(v.x, v.y), v.z);
        public static float Avg(this Vector3 v) => (v.x + v.y + v.z) / 3;
    }
}