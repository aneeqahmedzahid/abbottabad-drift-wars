using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using VehicleBehaviour;

public class SetupACPCarPrefab
{
    [MenuItem("Tools/Setup ACP Car Prefab")]
    public static void CreateACPCarPrefab()
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

        // WheelColliders + visual meshes + Suspension scripts
        var wcFL = CreateWheel("WC_FL", car.transform, new Vector3(-0.75f, 0.35f, 1.3f), true, true);
        var wcFR = CreateWheel("WC_FR", car.transform, new Vector3(0.75f, 0.35f, 1.3f), true, true);
        var wcRL = CreateWheel("WC_RL", car.transform, new Vector3(-0.75f, 0.35f, -1.3f), false, false);
        var wcRR = CreateWheel("WC_RR", car.transform, new Vector3(0.75f, 0.35f, -1.3f), false, false);

        // Center of Mass
        var com = new GameObject("CenterOfMass");
        com.transform.SetParent(car.transform);
        com.transform.localPosition = new Vector3(0, -0.45f, 0.1f);

        // Rigidbody
        var rb = car.AddComponent<Rigidbody>();
        rb.mass = 1150f;
        rb.drag = 0.05f;
        rb.angularDrag = 0.5f;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        // Set layer to Car for all children
        SetLayerRecursive(car, car.layer);

        // Add WheelVehicle (main ACP script)
        var wheelVehicle = car.AddComponent<WheelVehicle>();

        // Wire up wheels using reflection (properties are read-only)
        SetPrivateField(wheelVehicle, "driveWheel", new WheelCollider[] { wcRL, wcRR }); // RWD
        SetPrivateField(wheelVehicle, "turnWheel", new WheelCollider[] { wcFL, wcFR });  // Front steering

        // Set center of mass
        SetPrivateField(wheelVehicle, "centerOfMass", com.transform);

        // Set motor torque curve (AnimationCurve)
        var motorTorqueCurve = new AnimationCurve(
            new Keyframe(0, 200),
            new Keyframe(50, 300),
            new Keyframe(200, 0)
        );
        SetPrivateField(wheelVehicle, "motorTorque", motorTorqueCurve);

        // Set other parameters using properties (these have setters)
        wheelVehicle.DiffGearing = 4.0f;
        wheelVehicle.BrakeForce = 1500f;
        wheelVehicle.SteerAngle = 32f;
        wheelVehicle.SteerSpeed = 0.15f;
        wheelVehicle.JumpVel = 1.3f;
        wheelVehicle.DriftIntensity = 1f;
        wheelVehicle.Downforce = 2f;
        wheelVehicle.allowDrift = true;
        wheelVehicle.allowBoost = false; // Disable boost for now

        // Set turn input curve (private field)
        SetPrivateField(wheelVehicle, "turnInputCurve", AnimationCurve.Linear(-1.0f, -1.0f, 1.0f, 1.0f));

        // Create VehicleInputs asset
        var inputs = ScriptableObject.CreateInstance<VehicleInputs>();
        inputs.ThrottleInput = "Vertical";
        inputs.BrakeInput = "Brake";
        inputs.TurnInput = "Horizontal";
        inputs.JumpInput = "Jump";
        inputs.DriftInput = "Handbrake";
        inputs.BoostInput = "Boost";

        // Set the inputs reference
        SetPrivateField(wheelVehicle, "m_Inputs", inputs);

        // Add EngineSoundManager
        var audioSource = car.AddComponent<AudioSource>();
        audioSource.spatialBlend = 1f; // 3D sound
        var engineSound = car.AddComponent<EngineSoundManager>();

        // Save as prefab
        string prefabPath = "Assets/Prefabs/Car_ACP.prefab";
        var prefab = PrefabUtility.SaveAsPrefabAsset(car, prefabPath);
        Debug.Log($"ACP Car prefab created at {prefabPath}");

        // Clean up scene instance
        Object.DestroyImmediate(car);

        AssetDatabase.Refresh();
    }

    static void SetPrivateField(object obj, string fieldName, object value)
    {
        var field = obj.GetType().GetField(fieldName, 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
        if (field != null)
        {
            field.SetValue(obj, value);
        }
        else
        {
            Debug.LogWarning($"Field '{fieldName}' not found on {obj.GetType().Name}");
        }
    }

    static WheelCollider CreateWheel(string name, Transform parent, Vector3 localPos, bool isFront, bool isDrive)
    {
        var wheelGO = new GameObject(name);
        wheelGO.transform.SetParent(parent);
        wheelGO.transform.localPosition = localPos;

        // WheelCollider
        var wc = wheelGO.AddComponent<WheelCollider>();
        wc.mass = 20f;
        wc.radius = 0.33f;
        wc.suspensionDistance = 0.2f;
        var spring = wc.suspensionSpring;
        spring.spring = 35000f;
        spring.damper = 4500f;
        spring.targetPosition = 0.5f;
        wc.suspensionSpring = spring;
        var forwardFriction = wc.forwardFriction;
        forwardFriction.stiffness = 2f;
        wc.forwardFriction = forwardFriction;
        var sidewaysFriction = wc.sidewaysFriction;
        sidewaysFriction.stiffness = 2f;
        wc.sidewaysFriction = sidewaysFriction;

        // Visual mesh parent
        var meshParent = new GameObject("Mesh");
        meshParent.transform.SetParent(wheelGO.transform);
        meshParent.transform.localPosition = Vector3.zero;
        var cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cylinder.name = "Cylinder";
        cylinder.transform.SetParent(meshParent.transform);
        cylinder.transform.localPosition = Vector3.zero;
        cylinder.transform.localRotation = Quaternion.Euler(0, 0, 90);
        cylinder.transform.localScale = new Vector3(0.66f, 0.15f, 0.66f);
        Object.DestroyImmediate(cylinder.GetComponent<Collider>());

        // Suspension script
        var suspension = wheelGO.AddComponent<Suspension>();
        suspension.wheelModel = meshParent;
        suspension.localRotOffset = new Vector3(0, 0, 0);

        return wc;
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