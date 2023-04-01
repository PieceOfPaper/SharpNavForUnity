using System.Collections;
using System.Collections.Generic;

namespace SharpNav
{
    public static class UnityUtility
    {
        public static UnityEngine.Vector2 ToUnityVector2(this SharpNav.Geometry.Vector2 vector2) => new UnityEngine.Vector2(vector2.X, vector2.Y);
        public static UnityEngine.Vector3 ToUnityVector3(this SharpNav.Geometry.Vector3 vector3) => new UnityEngine.Vector3(vector3.X, vector3.Y, vector3.Z);


        public static SharpNav.Geometry.Vector2 ToSharpNavVector3(this UnityEngine.Vector2 vector3) => new SharpNav.Geometry.Vector2(vector3.x, vector3.y);
        public static SharpNav.Geometry.Vector3 ToSharpNavVector3(this UnityEngine.Vector3 vector3) => new SharpNav.Geometry.Vector3(vector3.x, vector3.y, vector3.z);

        public static IEnumerable<SharpNav.Geometry.Triangle3> ToSharpNavTriangles(this UnityEngine.MeshFilter meshFilter) => meshFilter == null ? null : ToSharpNavTriangles(meshFilter.transform, meshFilter.mesh);
        public static IEnumerable<SharpNav.Geometry.Triangle3> ToSharpNavTriangles(this UnityEngine.Mesh mesh) => ToSharpNavTriangles(null, mesh);
        public static IEnumerable<SharpNav.Geometry.Triangle3> ToSharpNavTriangles(UnityEngine.Transform transform, UnityEngine.Mesh mesh) => mesh == null ? null : ToSharpNavTriangles(transform, mesh.vertices, mesh.GetIndices(0));

        public static IEnumerable<SharpNav.Geometry.Triangle3> ToSharpNavTriangles(this UnityEngine.Terrain terrain) => terrain == null ? null : ToSharpNavTriangles(terrain.transform, terrain.terrainData);
        public static IEnumerable<SharpNav.Geometry.Triangle3> ToSharpNavTriangles(this UnityEngine.TerrainData terrainDaata) => ToSharpNavTriangles(null, terrainDaata);
        public static IEnumerable<SharpNav.Geometry.Triangle3> ToSharpNavTriangles(UnityEngine.Transform transform, UnityEngine.TerrainData terrainData)
        {
            int resolution = terrainData.heightmapResolution;
            var meshScale = terrainData.size / (resolution - 1);

            var vertices = new UnityEngine.Vector3[resolution * resolution];
            var indices = new int[(resolution - 1) * (resolution - 1) * 6];

            float[,] heights = terrainData.GetHeights(0, 0, resolution, resolution);

            for (int z = 0, i = 0; z < resolution; z++)
            {
                for (int x = 0; x < resolution; x++, i++)
                {
                    vertices[i] = new UnityEngine.Vector3(x * meshScale.x, heights[z, x] * meshScale.y, z * meshScale.z);
                    if ((i * 6 + 5) < indices.Length)
                    {
                        int topLeft = z * resolution + x;
                        int bottomLeft = (z + 1) * resolution + x;
                        int bottomRight = (z + 1) * resolution + x + 1;
                        int topRight = z * resolution + x + 1;

                        indices[i * 6] = topLeft;
                        indices[i * 6 + 1] = bottomRight;
                        indices[i * 6 + 2] = bottomLeft;
                        indices[i * 6 + 3] = topLeft;
                        indices[i * 6 + 4] = topRight;
                        indices[i * 6 + 5] = bottomRight;
                    }
                }
            }

            return ToSharpNavTriangles(transform, vertices, indices);
        }


        public static IEnumerable<SharpNav.Geometry.Triangle3> ToSharpNavTriangles(UnityEngine.Transform transform, UnityEngine.Vector3[] vertices, int[] indices)
        {
            if (vertices == null || indices == null)
                return null;

            var matrix = transform == null ? default : transform.localToWorldMatrix;
            var sharpNavVertices = new SharpNav.Geometry.Vector3[vertices.Length];
            for (int i = 0; i < vertices.Length; i++)
            {
                sharpNavVertices[i] = (transform == null ? vertices[i] : matrix.MultiplyPoint3x4(vertices[i])).ToSharpNavVector3();
            }

            // var triangles = SharpNav.Geometry.TriangleEnumerable.FromIndexedVector3(sharpNavVertices, indices, 0, 1, 0, indices.Length / 3);
            var triangles = new SharpNav.Geometry.Triangle3[indices.Length / 3];
            for (int i = 0; i < triangles.Length; i ++)
            {
                triangles[i] = new SharpNav.Geometry.Triangle3(sharpNavVertices[indices[i * 3]], sharpNavVertices[indices[i * 3 + 1]], sharpNavVertices[indices[i * 3 + 2]]);
            }

            return triangles;
        }
    }
}
