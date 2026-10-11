using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class SetupCarPrefab
{
    [MenuItem("Tools/Setup Car Prefab")]
    public static void CreateCarPrefab()
    {
        // Create Car root
        var car = new GameObject("Car");
        car.transform.position = new Vector3(0, 1, 0);
        car.layer = LayerMask.NameToLayer("Car");
        if (car.layer == -1)
        {
            Debug.LogError("Car layer not found! Please add 'Car' to User Layer 6 in Project Settings > Tags and Layers.");
            return;
        }

        // Body
        var body = GameObject.CreatePrimitive(PrimitiveType.Cube);
        body.name = "Body";
        body.transform.SetParent(car.transform);
        body.transform.localPosition = new Vector3(0, 0.5f, 0);
        body.transform.localScale = new Vector3(1.8f, 0.5f, 4.2f);
        // Keep BoxCollider

        // Cabin
        var cabin = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cabin.name = "Cabin";
        cabin.transform.SetParent(car.transform);
        cabin.transform.localPosition = new Vector3(0, 0.95f, -0.2f);
        cabin.transform.localScale = new Vector3(1.5f, 0.45f, 2f);
        Object.DestroyImmediate(cabin.GetComponent<Collider>());

        // WheelColliders
        var wcFL = CreateWheelCollider("WC_FL", car.transform, new Vector3(-0.75f, 0.35f, 1.3f));
        var wcFR = CreateWheelCollider("WC_FR", car.transform, new Vector3(0.75f, 0.35f, 1.3f));
        var wcRL = CreateWheelCollider("WC_RL", car.transform, new Vector3(-0.75f, 0.35f, -1.3f));
        var wcRR = CreateWheelCollider("WC_RR", car.transform, new Vector3(0.75f, 0.35f, -1.3f));

        // Visual wheel meshes
        var meshFL = CreateWheelMesh("Mesh_FL", car.transform, new Vector3(-0.75f, 0.35f, 1.3f));
        var meshFR = CreateWheelMesh("Mesh_FR", car.transform, new Vector3(0.75f, 0.35f, 1.3f));
        var meshRL = CreateWheelMesh("Mesh_RL", car.transform, new Vector3(-0.75f, 0.35f, -1.3f));
        var meshRR = CreateWheelMesh("Mesh_RR", car.transform, new Vector3(0.75f, 0.35f, -1.3f));

        // Rigidbody
        var rb = car.AddComponent<Rigidbody>();
        rb.mass = 1150f;
        rb.linearDamping = 0.05f;
        rb.angularDamping = 0.5f;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        // Set layer to Car for all children
        SetLayerRecursive(car, car.layer);

        // Add scripts
        var input = car.AddComponent<ADW.Vehicle.CarInput>();
        var controller = car.AddComponent<ADW.Vehicle.CarController>();

        // Wire up controller fields via reflection
        var controllerType = controller.GetType();
        controllerType.GetField("fl").SetValue(controller, wcFL);
        controllerType.GetField("fr").SetValue(controller, wcFR);
        controllerType.GetField("rl").SetValue(controller, wcRL);
        controllerType.GetField("rr").SetValue(controller, wcRR);
        controllerType.GetField("flMesh").SetValue(controller, meshFL.transform);
        controllerType.GetField("frMesh").SetValue(controller, meshFR.transform);
        controllerType.GetField("rlMesh").SetValue(controller, meshRL.transform);
        controllerType.GetField("rrMesh").SetValue(controller, meshRR.transform);

        // Save as prefab
        string prefabPath = "Assets/Prefabs/Car.prefab";
        var prefab = PrefabUtility.SaveAsPrefabAsset(car, prefabPath);
        Debug.Log($"Car prefab created at {prefabPath}");

        // Clean up scene instance
        Object.DestroyImmediate(car);

        AssetDatabase.Refresh();
    }

    static WheelCollider CreateWheelCollider(string name, Transform parent, Vector3 localPos)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent);
        go.transform.localPosition = localPos;
        var wc = go.AddComponent<WheelCollider>();
        wc.mass = 20f;
        wc.radius = 0.33f;
        wc.suspensionDistance = 0.2f;
        var spring = wc.suspensionSpring;
        spring.spring = 35000f;
        spring.damper = 4500f;
        spring.targetPosition = 0.5f;
        wc.suspensionSpring = spring;
        // Friction curves
        var forwardFriction = wc.forwardFriction;
        forwardFriction.stiffness = 2f;
        wc.forwardFriction = forwardFriction;
        var sidewaysFriction = wc.sidewaysFriction;
        sidewaysFriction.stiffness = 2f;
        wc.sidewaysFriction = sidewaysFriction;
        return wc;
    }

    static GameObject CreateWheelMesh(string name, Transform parent, Vector3 localPos)
    {
        var meshParent = new GameObject(name);
        meshParent.transform.SetParent(parent);
        meshParent.transform.localPosition = localPos;
        var cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cylinder.name = "Cylinder";
        cylinder.transform.SetParent(meshParent.transform);
        cylinder.transform.localPosition = Vector3.zero;
        cylinder.transform.localRotation = Quaternion.Euler(0, 0, 90); // Rotated 90 on Z
        cylinder.transform.localScale = new Vector3(0.66f, 0.15f, 0.66f);
        Object.DestroyImmediate(cylinder.GetComponent<Collider>());
        return meshParent;
    }

    static void SetLayerRecursive(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursive(child.gameObject, layer);
        }
    }
}