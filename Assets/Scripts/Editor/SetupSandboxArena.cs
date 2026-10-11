using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;

public class SetupSandboxArena
{
    [MenuItem("Tools/Setup Sandbox Arena")]
    public static void CreateSandboxArena()
    {
        // Open or create the scene
        var scenePath = "Assets/Scenes/Sandbox_Arena.unity";
        var scene = EditorSceneManager.OpenScene(scenePath);

        // Ensure Wall layer exists
        int wallLayer = LayerMask.NameToLayer("Wall");
        if (wallLayer == -1)
        {
            Debug.LogError("Wall layer not found! Please add 'Wall' to User Layer 8 in Project Settings > Tags and Layers.");
            return;
        }

        // Clear existing arena objects (optional - comment out if you want to keep existing)
        ClearExistingArenaObjects();

        // Create walls at ±150
        CreateWall("wall_N", new Vector3(0, 5, 150), new Vector3(300, 10, 1), wallLayer);  // North
        CreateWall("wall_S", new Vector3(0, 5, -150), new Vector3(300, 10, 1), wallLayer); // South
        CreateWall("wall_E", new Vector3(150, 5, 0), new Vector3(1, 10, 300), wallLayer);   // East
        CreateWall("wall_W", new Vector3(-150, 5, 0), new Vector3(1, 10, 300), wallLayer);  // West

        // Create figure-eight cones (~25m apart, 15-20 cones)
        CreateFigureEightCones(wallLayer);

        // Create ramp
        CreateRamp();

        // Ensure Directional Light exists
        EnsureDirectionalLight();

        // Save scene
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("Sandbox Arena setup complete!");
    }

    static void ClearExistingArenaObjects()
    {
        string[] namesToRemove = { "wall_N", "wall_S", "wall_E", "wall_W", "Ramp", "Cone_Group" };
        foreach (string name in namesToRemove)
        {
            var obj = GameObject.Find(name);
            if (obj != null) Object.DestroyImmediate(obj);
        }
        // Also clean up any cones left over
        var cones = GameObject.FindObjectsOfType<Transform>();
        foreach (var c in cones)
        {
            if (c.name.StartsWith("Cone_")) Object.DestroyImmediate(c.gameObject);
        }
    }

    static GameObject CreateWall(string name, Vector3 position, Vector3 scale, int layer)
    {
        var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = name;
        wall.transform.position = position;
        wall.transform.localScale = scale;
        wall.layer = layer;
        // Remove the default collider if you want a specific one, but Cube has BoxCollider
        // Make it static for better performance
        GameObjectUtility.SetStaticEditorFlags(wall, StaticEditorFlags.BatchingStatic | StaticEditorFlags.NavigationStatic);
        return wall;
    }

    static void CreateFigureEightCones(int wallLayer)
    {
        var coneGroup = new GameObject("Cone_Group");
        coneGroup.transform.position = Vector3.zero;

        // Figure-eight pattern: two loops crossing at center
        // Loop 1: centered at (-25, 0, 0), radius ~15
        // Loop 2: centered at (25, 0, 0), radius ~15
        // Total ~25m apart between loop centers

        List<Vector3> conePositions = new List<Vector3>();

        int conesPerLoop = 8; // 8 + 8 = 16 cones total
        float radius = 12.5f; // ~25m diameter

        // Left loop (negative X)
        Vector3 centerLeft = new Vector3(-25, 0, 0);
        for (int i = 0; i < conesPerLoop; i++)
        {
            float angle = (i / (float)conesPerLoop) * Mathf.PI * 2;
            Vector3 pos = centerLeft + new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
            conePositions.Add(pos);
        }

        // Right loop (positive X)
        Vector3 centerRight = new Vector3(25, 0, 0);
        for (int i = 0; i < conesPerLoop; i++)
        {
            float angle = (i / (float)conesPerLoop) * Mathf.PI * 2;
            Vector3 pos = centerRight + new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
            conePositions.Add(pos);
        }

        // Create cones
        for (int i = 0; i < conePositions.Count; i++)
        {
            var cone = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cone.name = $"Cone_{i:00}";
            cone.transform.position = conePositions[i] + Vector3.up * 0.5f; // Half height up
            cone.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            cone.transform.parent = coneGroup.transform;
            cone.layer = wallLayer; // Use Wall layer so they collide with Car
            // Cylinder already has a CapsuleCollider - good for cone shape
            GameObjectUtility.SetStaticEditorFlags(cone, StaticEditorFlags.BatchingStatic | StaticEditorFlags.NavigationStatic);
        }
    }

    static void CreateRamp()
    {
        var ramp = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ramp.name = "Ramp";
        ramp.transform.position = new Vector3(0, 0.25f, 0); // Slightly above ground
        ramp.transform.localScale = new Vector3(6, 0.5f, 10);
        ramp.transform.rotation = Quaternion.Euler(12, 0, 0); // X rotation 12 degrees
        ramp.layer = LayerMask.NameToLayer("Road") != -1 ? LayerMask.NameToLayer("Road") : 0;
        // Make it static
        GameObjectUtility.SetStaticEditorFlags(ramp, StaticEditorFlags.BatchingStatic | StaticEditorFlags.NavigationStatic);
    }

    static void EnsureDirectionalLight()
    {
        var light = GameObject.FindObjectOfType<Light>();
        if (light == null || light.type != LightType.Directional)
        {
            var lightObj = new GameObject("Directional Light");
            light = lightObj.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.0f;
            light.color = Color.white;
            light.transform.rotation = Quaternion.Euler(50, -30, 0);
        }
    }
}