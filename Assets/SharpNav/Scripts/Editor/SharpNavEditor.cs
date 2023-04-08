using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using SharpNav;

public class SharpNavEditor : EditorWindow
{
    [MenuItem("Window/AI/SharpNav")]
    public static void Open()
    {
        var editor = EditorWindow.GetWindow<SharpNavEditor>("SharpNav", true);
    }


    public Vector2 scroll = Vector2.zero;
    public SharpNav.IO.Json.NavMeshJsonSerializer serializer = null;
    public NavMeshGenerationSettings settings = null;
    public TiledNavMesh bakedNavmesh = null;
    public string bakeResultStr = string.Empty;

    public bool opt_findStaticMeshRenderer = true;
    public bool opt_findStaticTerrain = true;
    public Mesh opt_useMesh = null;

    private void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    private void OnDisable()
    {

        SceneView.duringSceneGui -= OnSceneGUI;
    }
    
    private void OnSceneGUI(SceneView sceneView)
    {
        OnDrawHandles();
    }


    #region OnGUI

    private void OnGUI()
    {
        if (settings == null)
            settings = NavMeshGenerationSettings.Default;

        if (serializer == null)
            serializer = new SharpNav.IO.Json.NavMeshJsonSerializer();

        scroll = EditorGUILayout.BeginScrollView(scroll);
        OnGUI_Settings();
        EditorGUILayout.Space();
        OnGUI_BakeOption();
        EditorGUILayout.EndScrollView();

        OnGUI_BakeResult();
        using (new EditorGUILayout.HorizontalScope())
        {
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Bake", GUILayout.ExpandWidth(false))) Bake();
            using (new EditorGUI.DisabledGroupScope(bakedNavmesh == null))
            {
                if (GUILayout.Button("Save (JSON)", GUILayout.ExpandWidth(false))) Save_Json();
                if (GUILayout.Button("Save (Binary)", GUILayout.ExpandWidth(false))) Save_Binary();
                if (GUILayout.Button("Clear", GUILayout.ExpandWidth(false))) Clear();
            }
        }
    }

    private void OnGUI_Settings()
    {
        if (settings == null)
            return;

        GUILayout.Label("Settings", EditorStyles.boldLabel);
        settings.CellSize = EditorGUILayout.FloatField("CellSize", settings.CellSize);
        settings.CellHeight = EditorGUILayout.FloatField("CellHeight", settings.CellHeight);
        settings.MaxClimb = EditorGUILayout.FloatField("MaxClimb", settings.MaxClimb);
        settings.AgentHeight = EditorGUILayout.FloatField("AgentHeight", settings.AgentHeight);
        settings.AgentRadius = EditorGUILayout.FloatField("AgentRadius", settings.AgentRadius);
        settings.MinRegionSize = EditorGUILayout.IntField("MinRegionSize", settings.MinRegionSize);
        settings.MergedRegionSize = EditorGUILayout.IntField("MergedRegionSize", settings.MergedRegionSize);
        settings.MaxEdgeLength = EditorGUILayout.IntField("MaxEdgeLength", settings.MaxEdgeLength);
        settings.MaxEdgeError = EditorGUILayout.FloatField("MaxEdgeError", settings.MaxEdgeError);
        settings.VertsPerPoly = EditorGUILayout.IntField("VertsPerPoly", settings.VertsPerPoly);
        settings.SampleDistance = EditorGUILayout.IntField("SampleDistance", settings.SampleDistance);
        settings.MaxSampleError = EditorGUILayout.IntField("MaxSampleError", settings.MaxSampleError);
        if (GUILayout.Button("Reset"))
        {
            settings = NavMeshGenerationSettings.Default;
        }
    }

    private void OnGUI_BakeOption()
    {
        GUILayout.Label("Bake Option", EditorStyles.boldLabel);
        opt_findStaticMeshRenderer = EditorGUILayout.Toggle("Find Static MeshRenderer", opt_findStaticMeshRenderer);
        opt_findStaticTerrain = EditorGUILayout.Toggle("Find Static Terrain", opt_findStaticTerrain);
        opt_useMesh = (Mesh)EditorGUILayout.ObjectField("Use Mesh", opt_useMesh, typeof(Mesh), true);
    }

    private void OnGUI_BakeResult()
    {
        if (bakedNavmesh == null)
            return;

        var newStyle = new GUIStyle(GUI.skin.box);
        newStyle.alignment = TextAnchor.MiddleLeft;
        GUILayout.Box(bakeResultStr, newStyle, GUILayout.ExpandWidth(true));
    }

    #endregion

    #region OnDrawHandles

    private void OnDrawHandles()
    {
        UnityEditorUtility.DrawNavMeshHandles(bakedNavmesh);
    }

    #endregion

    #region Process

    private void Bake()
    {
        var meshFilterList = new List<MeshFilter>();
        var terrainList = new List<Terrain>();
        var combineInstList = new List<CombineInstance>();

        if (opt_findStaticMeshRenderer == true)
        {
            var meshRenderers = GameObject.FindObjectsOfType<MeshRenderer>();
            foreach (var meshRenderer in meshRenderers)
            {
                if (meshRenderer == null) continue;
                if (UnityEditor.GameObjectUtility.AreStaticEditorFlagsSet(meshRenderer.gameObject, UnityEditor.StaticEditorFlags.NavigationStatic) == false) continue;

                var meshFilter = meshRenderer.GetComponent<MeshFilter>();
                if (meshFilter == null) continue;
                
                if (meshFilterList.Contains(meshFilter) == false)
                    meshFilterList.Add(meshFilter);
            }
        }
        if (opt_findStaticTerrain == true)
        {
            var terrains = GameObject.FindObjectsOfType<Terrain>();
            foreach (var terrain in terrains)
            {
                if (terrain == null) continue;
                if (UnityEditor.GameObjectUtility.AreStaticEditorFlagsSet(terrain.gameObject, UnityEditor.StaticEditorFlags.NavigationStatic) == false) continue;

                if (terrainList.Contains(terrain) == false)
                    terrainList.Add(terrain);
            }
        }
        if (Selection.objects != null)
        {
            for (int i = 0; i < Selection.gameObjects.Length; i++)
            {
                var meshRenderers = Selection.gameObjects[i].GetComponentsInChildren<MeshRenderer>();
                foreach (var meshRenderer in meshRenderers)
                {
                    if (meshRenderer == null) continue;

                    var meshFilter = meshRenderer.GetComponent<MeshFilter>();
                    if (meshFilter == null) continue;

                    if (meshFilterList.Contains(meshFilter) == false)
                        meshFilterList.Add(meshFilter);
                }
                var terrains = Selection.gameObjects[i].GetComponentsInChildren<Terrain>();
                foreach (var terrain in terrains)
                {
                    if (terrain == null) continue;

                    if (terrainList.Contains(terrain) == false)
                        terrainList.Add(terrain);
                }
            }
        }

        foreach (var meshFilter in meshFilterList)
        {
            if (meshFilter.sharedMesh == null) continue;
            var combineInst = new CombineInstance();
            combineInst.mesh = meshFilter.sharedMesh;
            combineInst.transform = meshFilter.transform.localToWorldMatrix;
            combineInstList.Add(combineInst);
        }
        foreach (var terrain in terrainList)
        {
            if (terrain.terrainData == null) continue;
            var combineInst = new CombineInstance();
            combineInst.mesh = terrain.terrainData.TerrainDataToMesh();
            combineInst.transform = terrain.transform.localToWorldMatrix;
            combineInstList.Add(combineInst);
        }
        if (opt_useMesh != null)
        {
            var combineInst = new CombineInstance();
            combineInst.mesh = opt_useMesh;
            combineInst.transform = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one);
            combineInstList.Add(combineInst);
        }
        var resultMesh = new Mesh();
        resultMesh.CombineMeshes(combineInstList.ToArray(), true, true, false);
        var triangles = resultMesh.ToSharpNavTriangles();

        try
        {
            bakedNavmesh = NavMesh.Generate(triangles, settings);
            var strBuilder = new System.Text.StringBuilder();
            strBuilder.AppendFormat("TileSize: {0}x{1}, TileCount: {2}", bakedNavmesh.TileWidth, bakedNavmesh.TileHeight, bakedNavmesh.TileCount);
            strBuilder.Append('\n');
            strBuilder.AppendFormat("MaxTiles: {0}, MaxPolys: {1}", bakedNavmesh.MaxTiles, bakedNavmesh.MaxPolys);
            bakeResultStr = strBuilder.ToString();
        }
        catch (System.Exception e)
        {
            bakedNavmesh = null;
            EditorUtility.DisplayDialog("SharpNav Bake Error", e.ToString(), "Ok");
            Debug.LogError(e);
        }
    }

    private void Save_Json()
    {
        if (bakedNavmesh == null)
            return;

        var lastSavePath = EditorPrefs.GetString("SharpNav_LastSavePath", Application.dataPath);
        var path = EditorUtility.SaveFilePanel("Save SharpNav NavMesh", lastSavePath, "NavMesh", "json");
        if (string.IsNullOrWhiteSpace(path)) return;

        EditorPrefs.SetString("SharpNav_LastSavePath", path);
        try
        {
            var text = serializer.SerializeToText(bakedNavmesh);
            System.IO.File.WriteAllText(path, text);
            AssetDatabase.Refresh();
        }
        catch (System.Exception e)
        {
            EditorUtility.DisplayDialog("SharpNav Save Error", e.ToString(), "Ok");
            Debug.LogError(e);
        }
    }

    private void Save_Binary()
    {
        if (bakedNavmesh == null)
            return;

        var lastSavePath = EditorPrefs.GetString("SharpNav_LastSavePath", Application.dataPath);
        var path = EditorUtility.SaveFilePanel("Save SharpNav NavMesh", lastSavePath, "NavMesh", "bytes");
        if (string.IsNullOrWhiteSpace(path)) return;

        EditorPrefs.SetString("SharpNav_LastSavePath", path);
        try
        {
            var bytes = serializer.SerializeToBinary(bakedNavmesh);
            System.IO.File.WriteAllBytes(path, bytes);
            AssetDatabase.Refresh();
        }
        catch (System.Exception e)
        {
            EditorUtility.DisplayDialog("SharpNav Save Error", e.ToString(), "Ok");
            Debug.LogError(e);
        }
    }

    private void Clear()
    {
        bakeResultStr = string.Empty;
        bakedNavmesh = null;
        System.GC.Collect();
    }

    #endregion
}
