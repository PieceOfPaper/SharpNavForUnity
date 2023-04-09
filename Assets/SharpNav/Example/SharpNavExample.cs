using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SharpNav;

public class SharpNavExample : MonoBehaviour
{
    public int groupID = 1;
    public TextAsset navAsset;
    public Transform startPoint;
    public Transform dstPoint;
    public Vector3 extends = Vector3.one;
    public SharpNavAgent agent;

    private SharpNavMesh navMesh;
    private Vector3[] path;

    // Start is called before the first frame update
    private void Start()
    {
        navMesh = SharpNavManager.Instance.LoadNavMesh(groupID, navAsset);
        agent.MoveTo(dstPoint.position);

        path = navMesh.FindPath(startPoint.position, dstPoint.position, extends);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        if (navMesh != null && navMesh.NavMeshData != null)
        {
            var tile = navMesh.NavMeshData.GetTileAt(0, 0, 0);
            for (int polyIndex = 0; polyIndex < tile.Polys.Length; polyIndex++)
            {
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

        Gizmos.color = Color.blue;
        if (path != null && path.Length > 0)
        {
            Gizmos.DrawSphere(path[0], 0.1f);
            for (int i = 1; i < path.Length; i++)
            {
                Gizmos.DrawLine(path[i - 1], path[i]);
                Gizmos.DrawSphere(path[i], 0.1f);
            }
        }
    }
}
