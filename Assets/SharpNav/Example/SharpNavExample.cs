using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SharpNavExample : MonoBehaviour
{
    //TODO
    //Example을 여러 단계로 나누어서 쓰자.
    //1. SharpNav.NavMesh 생성
    //2. SharpNav.NavMesh로 경로탐색, Raycast 등등
    //3. SharpNav.NavMesh 저장 및 불러오기 => 이 부분은 기존 코드 활용 못하니, 새로 만들어줘야한다.

    private SharpNav.NavMesh navMesh;

    // Start is called before the first frame update
    private void Start()
    {
        //prepare the geometry from your mesh data
        var tris = SharpNav.Geometry.TriangleEnumerable.FromIndexedVector3(null, null, 0, 1, 0, 0);

        //use the default generation settings
        var settings = SharpNav.NavMeshGenerationSettings.Default;
        settings.AgentHeight = 1.7f;
        // settings.AgentWidth = 0.6f;

        //generate the mesh
        navMesh = SharpNav.NavMesh.Generate(tris, settings);
    }
    
    private void OnDrawGizmos()
    {
        if (navMesh != null)
        {
            //TODO - 네브메시를 그려보자.
        }
    }
}
