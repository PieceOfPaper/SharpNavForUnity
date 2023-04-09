using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SharpNav;
using SharpNav.Crowds;

public class SharpNavAgent : MonoBehaviour
{
    [SerializeField] private int m_GroupID = 1;
    [SerializeField] public float Radius = 0.6f;
    [SerializeField] public float Height = 2.0f;
    [SerializeField] public float MaxAcceleration = 8.0f;
    [SerializeField] public float MaxSpeed = 3.5f;
    [SerializeField] public float CollisionQueryRange = 0.6f * 30.0f;
    [SerializeField] public float PathOptimizationRange = 0.6f * 30.0f;
    [SerializeField] public float SeparationWeight;
    [SerializeField] public UpdateFlags UpdateFlags = new UpdateFlags();
    [SerializeField] public byte ObstacleAvoidanceType;
    [SerializeField] public byte QueryFilterType;


    public int GroupID
    {
        get { return m_GroupID; }
        set
        {
            if (value != m_GroupID)
            {
#if UNITY_EDITOR
                if (UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode == false)
                {
                    m_GroupID = value;
                    return;
                }
#endif
                if (enabled == true)
                {
                    UnregistAgent();
                    m_GroupID = value;
                    RegistAgent();
                }
                else
                {
                    m_GroupID = value;
                }
            }
        }
    }

    public AgentParams Params => new AgentParams() {
        Radius = Radius,
        Height = Height,
        MaxAcceleration = MaxAcceleration,
        MaxSpeed = MaxSpeed,
        CollisionQueryRange = CollisionQueryRange,
        PathOptimizationRange = PathOptimizationRange,
        SeparationWeight = SeparationWeight,
        UpdateFlags = UpdateFlags,
        ObstacleAvoidanceType = ObstacleAvoidanceType,
        QueryFilterType = QueryFilterType,
    };


    private Agent m_Agent = null;
    public Vector3 Position => m_Agent == null ? transform.position : m_Agent.Position.ToUnityVector3();
    public Vector3 TargetPosition => m_Agent == null ? m_TargetPosition : m_Agent.TargetPosition.ToUnityVector3();

    private int m_AgentIndex = -1;
    public int AgentIndex => m_AgentIndex;

    private Vector3 m_TargetPosition;
    private Vector3 m_PrevPosition;


    private void Awake()
    {
        m_TargetPosition = transform.position;
        m_PrevPosition = transform.position;
    }

    private void OnEnable()
    {
        RegistAgent();
    }

    private void OnDisable()
    {
        UnregistAgent();
    }

    private void Update()
    {
        if (m_Agent == null)
            return;

        switch (m_Agent.State)
        {
            case AgentState.Walking:
                {
                    var agentTargetPosition = m_Agent.Corridor.Target.ToUnityVector3();
                    if (Mathf.Abs(m_TargetPosition.x - agentTargetPosition.x) > 0.0001f || Mathf.Abs(m_TargetPosition.z - agentTargetPosition.z) > 0.0001f)
                    {
                        MoveTo(m_TargetPosition);
                    }

                    var agentPos = m_Agent.Position.ToUnityVector3();
                    transform.position = agentPos;
                    transform.rotation = Quaternion.LookRotation(agentPos - m_PrevPosition);
                    m_PrevPosition = agentPos;
                }
                break;
        }
    }


    private void RegistAgent()
    {
        if (SharpNavManager.Instance == null) return;
        SharpNavManager.Instance.RegistAgent(this, out m_Agent, out m_AgentIndex);
    }

    private void UnregistAgent()
    {
        if (SharpNavManager.Instance == null) return;
        SharpNavManager.Instance.UnegistAgent(this);
    }

    public void RefreshRegist()
    {
        if (enabled == false)
            return;

        UnregistAgent();
        RegistAgent();
    }


    public bool MoveTo(Vector3 pos)
    {
        if (m_Agent == null)
            return false;

        var navMesh = SharpNavManager.Instance.GetNavMeshByGroupID(m_GroupID);
        if (navMesh == null)
            return false;

        m_TargetPosition = pos;
        var navPos = pos.ToSharpNavVector3();
        var nearlestNavPoint = navMesh.Query.FindNearestPoly(navPos, new SharpNav.Geometry.Vector3(1f, 20f, 1f));
        return m_Agent.RequestMoveTarget(nearlestNavPoint.Polygon, nearlestNavPoint.Position);
    }


#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (m_Agent != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawCube(m_Agent.Position.ToUnityVector3(), new Vector3(0.5f, 2.0f, 0.5f));
            Gizmos.DrawCube(m_Agent.TargetPosition.ToUnityVector3(), new Vector3(0.5f, 2.0f, 0.5f));

            Gizmos.color = Color.cyan;
            // Gizmos.DrawCube(agent.Corridor.Pos.ToUnityVector3(), new Vector3(0.5f, 2.0f, 0.5f));
            Gizmos.DrawCube(m_Agent.Corridor.Target.ToUnityVector3(), new Vector3(0.5f, 2.0f, 0.5f));
        }
    }
#endif

}
