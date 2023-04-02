using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathBuilder
{
    private Vector3 m_PrevDir;
    private Vector3 m_PrevPos;
    private List<Vector3> m_PathList = new List<Vector3>();

    public void AppendPosition(Vector3 pos)
    {
        var dir = pos - m_PrevPos;
        if (m_PathList.Count > 1)
        {
            var rate_x = dir.x / m_PrevDir.x;
            // var rate_y = dir.y / m_PrevDir.y;
            var rate_z = dir.z / m_PrevDir.z;
            if (Mathf.Abs(rate_x - rate_z) < 0.0001f)
                m_PathList[m_PathList.Count - 1] = pos;
            else
                m_PathList.Add(pos);
        }
        else
        {
            m_PathList.Add(pos);
        }
        m_PrevPos = pos;
        m_PrevDir = dir;
    }

    public Vector3[] ToArray() => m_PathList.ToArray();

    public void Clear()
    {
        m_PrevDir = Vector3.zero;
        m_PrevPos = Vector3.zero;
        m_PathList.Clear();
    }
}
