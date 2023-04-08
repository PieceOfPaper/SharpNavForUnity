using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SharpNav;
using SharpNav.Pathfinding;

/// <summary>
/// Unity용 NavMesh Container
/// </summary>
public class SharpNavMesh
{
    private TiledNavMesh m_NavMeshData;
    public TiledNavMesh NavMeshData => m_NavMeshData;

    private NavMeshQuery m_Query;
    private PathBuilder m_PathBuilder;

    public SharpNavMesh(TiledNavMesh navMesh)
    {
        m_NavMeshData = navMesh;
        m_Query = new NavMeshQuery(navMesh, 2048);
        m_PathBuilder = new PathBuilder();
    }

    public Vector3 GetNearlestPosition(Vector3 pos, Vector3 extends)
    {
        var navPoint = m_Query.FindNearestPoly(pos.ToSharpNavVector3(), extends.ToSharpNavVector3());
        return navPoint.Position.ToUnityVector3();
    }

    public Vector3[] FindPath(Vector3 startPos, Vector3 dstPos, Vector3 extends)
    {
        var startNavPos = startPos.ToSharpNavVector3();
        var dstNavPos = dstPos.ToSharpNavVector3();
        var startNearlestNavPoint = m_Query.FindNearestPoly(startNavPos, extends.ToSharpNavVector3());
        var dstNearlestNavPoint = m_Query.FindNearestPoly(dstNavPos, extends.ToSharpNavVector3());

        var path = new Path();
        var filter = new NavQueryFilter();
        var findPathResult = m_Query.FindPath(ref startNearlestNavPoint, ref dstNearlestNavPoint, filter, path);

        //find a smooth path over the mesh surface
        int npolys = path.Count;
        var iterPos = new SharpNav.Geometry.Vector3();
        var targetPos = new SharpNav.Geometry.Vector3();
        m_Query.ClosestPointOnPoly(path[0], startNavPos, ref iterPos);
        m_Query.ClosestPointOnPoly(path[npolys - 1], dstNavPos, ref targetPos);

        m_PathBuilder.Clear();
        m_PathBuilder.AppendPosition(iterPos.ToUnityVector3());

        float STEP_SIZE = 0.5f;
        float SLOP = 0.01f;
        int loopCount = 0;
        while (npolys > 0 && loopCount < 10000)
        {
            //find location to steer towards
            var steerPos = new SharpNav.Geometry.Vector3();
            StraightPathFlags steerPosFlag = 0;
            var steerPosRef = NavPolyId.Null;

            if (!SharpNav.QueryUtility.GetSteerTarget(m_Query, iterPos, targetPos, SLOP, path, ref steerPos, ref steerPosFlag, ref steerPosRef))
                break;

            bool endOfPath = (steerPosFlag & StraightPathFlags.End) != 0 ? true : false;
            bool offMeshConnection = (steerPosFlag & StraightPathFlags.OffMeshConnection) != 0 ? true : false;

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
            var visited = new List<NavPolyId>(16);
            var startPoint = new NavPoint(path[0], iterPos);
            m_Query.MoveAlongSurface(ref startPoint, ref moveTgt, out result, visited);
            path.FixupCorridor(visited);
            npolys = path.Count;
            float h = 0;
            m_Query.GetPolyHeight(path[0], result, ref h);
            result.Y = h;
            iterPos = result;

            //handle end of path when close enough
            if (endOfPath && SharpNav.QueryUtility.InRange(iterPos, steerPos, SLOP, 1.0f))
            {
                //reached end of path
                iterPos = targetPos;
                m_PathBuilder.AppendPosition(iterPos.ToUnityVector3());
                break;
            }

            //store results
            m_PathBuilder.AppendPosition(iterPos.ToUnityVector3());

            loopCount++;
        }
        return m_PathBuilder.ToArray();
    }



    public static SharpNavMesh Load(TextAsset textAsset)
    {
        return Load(textAsset.bytes);
    }

    public static SharpNavMesh Load(string text)
    {
        var navMesh = new SharpNav.IO.Json.NavMeshJsonSerializer().DeserializeFromText(text);
        return new SharpNavMesh(navMesh);
    }

    public static SharpNavMesh Load(byte[] bytes)
    {
        var navMesh = new SharpNav.IO.Json.NavMeshJsonSerializer().DeserializeFromBinary(bytes);
        return new SharpNavMesh(navMesh);
    }
}
