using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SharpNav;

public class SharpNavExample : MonoBehaviour
{
    public Transform startPoint;
    public Transform dstPoint;
    public Vector3 extends = Vector3.one;

    //TODO
    //Example을 여러 단계로 나누어서 쓰자.
    //1. SharpNav.NavMesh 생성
    //2. SharpNav.NavMesh로 경로탐색, Raycast 등등
    //3. SharpNav.NavMesh 저장 및 불러오기 => 이 부분은 기존 코드 활용 못하니, 새로 만들어줘야한다.

    private SharpNav.TiledNavMesh navMesh;
    private SharpNav.Pathfinding.Path path;
    private List<Vector3> smoothPath = new List<Vector3>(2048);

    // Start is called before the first frame update
    private void Start()
    {
        navMesh = new SharpNav.IO.Json.NavMeshJsonSerializer().Deserialize(System.IO.Path.Combine(Application.dataPath, "SharpNav/Example/NavMesh.json"));

        var query = new SharpNav.NavMeshQuery(navMesh, 2048);

        if (startPoint != null && dstPoint != null)
        {
            var startPos = startPoint.position.ToSharpNavVector3();
            var dstPos = dstPoint.position.ToSharpNavVector3();
            var startNearlestNavPoint = query.FindNearestPoly(startPos, extends.ToSharpNavVector3());
            var dstNearlestNavPoint = query.FindNearestPoly(dstPos, extends.ToSharpNavVector3());

            path = new SharpNav.Pathfinding.Path();
            var filter = new SharpNav.Pathfinding.NavQueryFilter();
            var findPathResult = query.FindPath(ref startNearlestNavPoint, ref dstNearlestNavPoint, filter, path);
            Debug.Log("FindPath " + findPathResult);

			//find a smooth path over the mesh surface
			int npolys = path.Count;
			var iterPos = new SharpNav.Geometry.Vector3();
			var targetPos = new SharpNav.Geometry.Vector3();
			query.ClosestPointOnPoly(path[0], startPos, ref iterPos);
			query.ClosestPointOnPoly(path[npolys - 1], dstPos, ref targetPos);

            smoothPath.Clear();
            smoothPath.Add(iterPos.ToUnityVector3());

			float STEP_SIZE = 0.5f;
			float SLOP = 0.01f;
			while (npolys > 0 && smoothPath.Count < smoothPath.Capacity)
			{
				//find location to steer towards
				var steerPos = new SharpNav.Geometry.Vector3();
                SharpNav.Pathfinding.StraightPathFlags steerPosFlag = 0;
				var steerPosRef = SharpNav.Pathfinding.NavPolyId.Null;

				if (!SharpNav.QueryUtility.GetSteerTarget(query, iterPos, targetPos, SLOP, path, ref steerPos, ref steerPosFlag, ref steerPosRef))
					break;

				bool endOfPath = (steerPosFlag & SharpNav.Pathfinding.StraightPathFlags.End) != 0 ? true : false;
				bool offMeshConnection = (steerPosFlag & SharpNav.Pathfinding.StraightPathFlags.OffMeshConnection) != 0 ? true : false;

				//find movement delta
				var delta = steerPos - iterPos;
                float len = (float)System.Math.Sqrt(SharpNav.Geometry.Vector3.Dot(delta, delta));

				//if steer target is at end of path or off-mesh link
				//don't move past location
				if ((endOfPath || offMeshConnection) && len < STEP_SIZE)
					len = 1;
				else
					len = STEP_SIZE / len;

				var moveTgt = new SharpNav.Geometry.Vector3();
				SharpNav.QueryUtility.VMad(ref moveTgt, iterPos, delta, len);

				//move
				var result = new SharpNav.Geometry.Vector3();
				var visited = new List<SharpNav.Pathfinding.NavPolyId>(16);
				var startPoint = new SharpNav.Pathfinding.NavPoint(path[0], iterPos);
				query.MoveAlongSurface(ref startPoint, ref moveTgt, out result, visited);
				path.FixupCorridor(visited);
				npolys = path.Count;
				float h = 0;
				query.GetPolyHeight(path[0], result, ref h);
				result.Y = h;
				iterPos = result;

				//handle end of path when close enough
				if (endOfPath && SharpNav.QueryUtility.InRange(iterPos, steerPos, SLOP, 1.0f))
				{
					//reached end of path
					iterPos = targetPos;
					if (smoothPath.Count < smoothPath.Capacity)
					{
						smoothPath.Add(iterPos.ToUnityVector3());
					}
					break;
				}

				//store results
				if (smoothPath.Count < smoothPath.Capacity)
				{
                    smoothPath.Add(iterPos.ToUnityVector3());
				}
			}
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        if (navMesh != null)
        {
            var tile = navMesh.GetTileAt(0, 0, 0);
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

        Gizmos.color = Color.green;
        if (smoothPath.Count > 0)
        {
            Gizmos.DrawSphere(smoothPath[0], 0.1f);
            for (int i = 1; i < smoothPath.Count; i++)
            {
                Gizmos.DrawLine(smoothPath[i - 1], smoothPath[i]);
                Gizmos.DrawSphere(smoothPath[i], 0.1f);
            }
        }
    }
}
