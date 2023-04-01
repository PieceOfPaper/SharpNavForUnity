using System.Collections;
using System.Collections.Generic;

namespace SharpNav
{
    public static class UnityUtility
    {
        public static SharpNav.Geometry.Vector2 ToSharpNavVector3(this UnityEngine.Vector2 vector3) => new SharpNav.Geometry.Vector2(vector3.x, vector3.y);
        public static SharpNav.Geometry.Vector3 ToSharpNavVector3(this UnityEngine.Vector3 vector3) => new SharpNav.Geometry.Vector3(vector3.x, vector3.y, vector3.z);

        public static IEnumerable<SharpNav.Geometry.Triangle3> ToSharpNavTriangles(this UnityEngine.MeshFilter meshFilter) => meshFilter == null ? null : ToSharpNavTriangles(meshFilter.transform, meshFilter.mesh);
        public static IEnumerable<SharpNav.Geometry.Triangle3> ToSharpNavTriangles(this UnityEngine.Mesh mesh) => ToSharpNavTriangles(null, mesh);

        public static IEnumerable<SharpNav.Geometry.Triangle3> ToSharpNavTriangles(UnityEngine.Transform transform, UnityEngine.Mesh mesh) => mesh == null ? null : ToSharpNavTriangles(transform, mesh.vertices, mesh.GetIndices(0));
        public static IEnumerable<SharpNav.Geometry.Triangle3> ToSharpNavTriangles(UnityEngine.Transform transform, UnityEngine.Vector3[] vertices, int[] indices)
        {
            if (vertices == null || indices == null)
                return null;

            var localToWorldMatrix = transform == null ? default : transform.localToWorldMatrix;
            var sharpNavVertices = new SharpNav.Geometry.Vector3[vertices.Length];
            for (int i = 0; i < vertices.Length; i++)
            {
                sharpNavVertices[i] = (transform == null ? vertices[i] : localToWorldMatrix.MultiplyPoint3x4(vertices[i])).ToSharpNavVector3();
            }

            // var triangles = SharpNav.Geometry.TriangleEnumerable.FromIndexedVector3(sharpNavVertices, indices, 0, 1, 0, indices.Length / 3);
            var triangles = new SharpNav.Geometry.Triangle3[indices.Length / 3];
            for (int i = 0; i < triangles.Length; i ++)
            {
                triangles[i] = new SharpNav.Geometry.Triangle3(sharpNavVertices[indices[i]], sharpNavVertices[indices[i + 1]], sharpNavVertices[indices[i + 2]]);
            }

            return triangles;
        }
    }
}
