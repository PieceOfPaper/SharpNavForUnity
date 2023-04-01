using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SharpNav;

public class SharpNavExample : MonoBehaviour
{
    //TODO
    //Example을 여러 단계로 나누어서 쓰자.
    //1. SharpNav.NavMesh 생성
    //2. SharpNav.NavMesh로 경로탐색, Raycast 등등
    //3. SharpNav.NavMesh 저장 및 불러오기 => 이 부분은 기존 코드 활용 못하니, 새로 만들어줘야한다.

    private SharpNav.NavMesh navMesh;
    private List<SharpNav.Geometry.Triangle3> triangles = new List<SharpNav.Geometry.Triangle3>();

    // Start is called before the first frame update
    private void Start()
    {
        triangles.Clear();
        var meshRenderers = GameObject.FindObjectsOfType<MeshRenderer>();
        var combineInstList = new List<CombineInstance>();
        foreach (var meshRenderer in meshRenderers)
        {
            if (meshRenderer == null) continue;
            // if (UnityEditor.GameObjectUtility.AreStaticEditorFlagsSet(meshRenderer.gameObject, UnityEditor.StaticEditorFlags.NavigationStatic) == false) continue;
            
            var meshFilter = meshRenderer.GetComponent<MeshFilter>();
            if (meshFilter == null) continue;

            var combineInst = new CombineInstance();
            combineInst.mesh = meshFilter.mesh;
            combineInst.transform = meshFilter.transform.localToWorldMatrix;
            combineInstList.Add(combineInst);
        }

        var resultMesh = new Mesh();
        resultMesh.CombineMeshes(combineInstList.ToArray(), true, true, false);
        triangles.AddRange(resultMesh.ToSharpNavTriangles());


        //generate the mesh
        var setting = NavMeshGenerationSettings.Default;
        setting.AgentRadius = 0.2f;
        navMesh = NavMesh.Generate(triangles, setting);

        // var query = new SharpNav.NavMeshQuery(navMesh, 2048);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        foreach (var triangle in triangles)
        {
            var v1 = triangle.A.ToUnityVector3();
            var v2 = triangle.B.ToUnityVector3();
            var v3 = triangle.C.ToUnityVector3();

            Gizmos.DrawLine(v1, v2);
            Gizmos.DrawLine(v2, v3);
            Gizmos.DrawLine(v3, v1);
            Gizmos.DrawSphere(v1, 0.1f);
            Gizmos.DrawSphere(v2, 0.1f);
            Gizmos.DrawSphere(v3, 0.1f);
        }

        Gizmos.color = Color.red;
        if (navMesh != null)
        {
            for (int tileY = 0; tileY < navMesh.TileHeight; tileY++)
            {
                for (int tileX = 0; tileX < navMesh.TileWidth; tileX++)
                {
                    var tile = navMesh.GetTileAt(tileX, tileY, 0);
                    if (tile == null || tile.Polys == null) continue;
                    for (int polyIndex = 0; polyIndex < tile.Polys.Length; polyIndex++)
                    {
                        // for (int vertIndex = 0; (vertIndex + 2) < tile.Polys[polyIndex].Verts.Length; vertIndex += 3)
                        // {
                        //     int vertIndex0 = tile.Polys[polyIndex].Verts[vertIndex];
                        //     int vertIndex1 = tile.Polys[polyIndex].Verts[vertIndex + 1];
                        //     int vertIndex2 = tile.Polys[polyIndex].Verts[vertIndex + 2];

                        //     var v1 = tile.Verts[vertIndex0].ToUnityVector3();
                        //     var v2 = tile.Verts[vertIndex1].ToUnityVector3();
                        //     var v3 = tile.Verts[vertIndex2].ToUnityVector3();

                        //     Gizmos.DrawLine(v1, v2);
                        //     Gizmos.DrawLine(v2, v3);
                        //     Gizmos.DrawLine(v3, v1);
                        //     Gizmos.DrawSphere(v1, 0.1f);
                        //     Gizmos.DrawSphere(v2, 0.1f);
                        //     Gizmos.DrawSphere(v3, 0.1f);
                        // }
                        for (int vertIndex = 2; vertIndex < SharpNav.Pathfinding.PathfindingCommon.VERTS_PER_POLYGON; vertIndex++)
                        {
                            if (tile.Polys[polyIndex].Verts[vertIndex] == 0)
                                break;

                            int vertIndex0 = tile.Polys[polyIndex].Verts[0];
                            int vertIndex1 = tile.Polys[polyIndex].Verts[vertIndex - 1];
                            int vertIndex2 = tile.Polys[polyIndex].Verts[vertIndex];

                            var v1 = tile.Verts[vertIndex0].ToUnityVector3();
                            var v2 = tile.Verts[vertIndex1].ToUnityVector3();
                            var v3 = tile.Verts[vertIndex2].ToUnityVector3();

                            Gizmos.DrawLine(v1, v2);
                            Gizmos.DrawLine(v2, v3);
                            Gizmos.DrawLine(v3, v1);
                            Gizmos.DrawSphere(v1, 0.1f);
                            Gizmos.DrawSphere(v2, 0.1f);
                            Gizmos.DrawSphere(v3, 0.1f);
                        }
                    }
                }
            }
        }
    }
}
