using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using SharpNav;

public class SharpNavManager
{
    private static SharpNavManager m_Instance;
    public static SharpNavManager Instance
    {
        get
        {
#if UNITY_EDITOR
            if (UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode == false)
                return null;
#endif
            if (m_Instance == null)
                m_Instance = new SharpNavManager();
            return m_Instance;
        }
    }

    public SharpNavManager()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
        SceneManager.activeSceneChanged += OnActiveSceneChanged;
    }

    ~SharpNavManager()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
        SceneManager.activeSceneChanged -= OnActiveSceneChanged;
    }

    private void OnSceneLoaded(Scene scene, UnityEngine.SceneManagement.LoadSceneMode loadSceneMode)
    {
        if (m_Instance == null || m_Instance != this)
            return;
    }

    private void OnSceneUnloaded(Scene scene)
    {
        if (m_Instance == null || m_Instance != this)
            return;
    }

    private void OnActiveSceneChanged(Scene prevScene, Scene nextScene)
    {
        if (m_Instance == null || m_Instance != this)
            return;
    }



    #region  NavMesh

    private Dictionary<int, SharpNavMesh> m_NavMeshs = new Dictionary<int, SharpNavMesh>();

    public SharpNavMesh LoadNavMesh(int groupID, TextAsset textAsset)
    {
        return LoadNavMesh(groupID, textAsset.bytes);
    }

    public SharpNavMesh LoadNavMesh(int groupID, string text)
    {
        var navMeshData = new SharpNav.IO.Json.NavMeshJsonSerializer().DeserializeFromText(text);
        var navMesh = new SharpNavMesh(groupID, navMeshData);
        AppendNevMesh(navMesh);
        return navMesh;
    }

    public SharpNavMesh LoadNavMesh(int groupID, byte[] bytes)
    {
        var navMeshData = new SharpNav.IO.Json.NavMeshJsonSerializer().DeserializeFromBinary(bytes);
        var navMesh = new SharpNavMesh(groupID, navMeshData);
        AppendNevMesh(navMesh);
        return navMesh;
    }

    public void AppendNevMesh(SharpNavMesh navMesh)
    {
        if (m_NavMeshs.ContainsKey(navMesh.GroupID))
            m_NavMeshs.Remove(navMesh.GroupID);

        m_NavMeshs.Add(navMesh.GroupID, navMesh);

        if (m_Agents.ContainsKey(navMesh.GroupID))
        {
            var registedAgents = m_Agents[navMesh.GroupID].ToArray();
            foreach (var agent in registedAgents)
            {
                if (agent == null) continue;
                agent.RefreshRegist();
            }
        }
    }

    public SharpNavMesh GetNavMeshByGroupID(int groupID)
    {
        if (m_NavMeshs.ContainsKey(groupID) == false)
            return null;

        return m_NavMeshs[groupID];
    }

    #endregion


    #region Agent

    private Dictionary<int, List<SharpNavAgent>> m_Agents = new Dictionary<int, List<SharpNavAgent>>();

    public bool RegistAgent(SharpNavAgent agentBehaviour, out SharpNav.Crowds.Agent agent, out int agentIndex)
    {
        if (m_Agents.ContainsKey(agentBehaviour.GroupID) == false)
            m_Agents.Add(agentBehaviour.GroupID, new List<SharpNavAgent>());

        m_Agents[agentBehaviour.GroupID].Add(agentBehaviour);

        agent = null;
        agentIndex = -1;
        var navMesh = GetNavMeshByGroupID(agentBehaviour.GroupID);
        if (navMesh == null)
            return false;

        agentIndex = navMesh.Crowd.AddAgent(agentBehaviour.transform.position.ToSharpNavVector3(), agentBehaviour.Params);
        agent = navMesh.Crowd.GetAgent(agentIndex);
        return true;
    }

    public void UnegistAgent(SharpNavAgent agentBehaviour)
    {
        if (m_Agents.ContainsKey(agentBehaviour.GroupID) == false)
            m_Agents.Add(agentBehaviour.GroupID, new List<SharpNavAgent>());

        m_Agents[agentBehaviour.GroupID].Remove(agentBehaviour);

        if (agentBehaviour.AgentIndex >= 0)
        {
            var navMesh = GetNavMeshByGroupID(agentBehaviour.GroupID);
            if (navMesh != null)
            {
                navMesh.Crowd.RemoveAgent(agentBehaviour.AgentIndex);
            }
        }
    }

    #endregion
}
