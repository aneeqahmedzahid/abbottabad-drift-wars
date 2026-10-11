using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;


public class SetupCinemachineCamera
{
    [MenuItem("Tools/Setup Cinemachine Chase Camera")]
    public static void CreateChaseCamera()
    {
        // Open Sandbox_Arena scene
        var scenePath = "Assets/Scenes/Sandbox_Arena.unity";
        var scene = EditorSceneManager.OpenScene(scenePath);

        // Find the Car in the scene
        var car = GameObject.Find("Car");
        if (car == null)
        {
            Debug.LogError("Car not found in scene! Please add the Car prefab to the scene first.");
            return;
        }

        // Get or create the main camera
        var mainCamera = Camera.main;
        if (mainCamera == null)
        {
            var camGo = new GameObject("Main Camera");
            mainCamera = camGo.AddComponent<Camera>();
            camGo.tag = "MainCamera";
        }

        // Add CinemachineBrain to main camera if not present
        var brain = mainCamera.GetComponent<Unity.Cinemachine.CinemachineBrain>();
        if (brain == null)
        {
            brain = mainCamera.gameObject.AddComponent<Unity.Cinemachine.CinemachineBrain>();
        }

        // Create Cinemachine Virtual Camera (2.x API)
        var vcamGo = new GameObject("CM_ChaseCamera");
        var vcam = vcamGo.AddComponent<Unity.Cinemachine.CinemachineVirtualCamera>();
        vcam.Priority = 10;

        // Set Follow and LookAt to the Car
        vcam.Follow = car.transform;
        vcam.LookAt = car.transform;

        // Configure Body (Transposer) for chase camera
        var transposer = vcam.GetCinemachineComponent<Unity.Cinemachine.CinemachineTransposer>();
        if (transposer == null)
        {
            transposer = vcam.AddCinemachineComponent<Unity.Cinemachine.CinemachineTransposer>();
        }
        transposer.m_FollowOffset = new Vector3(0, 3.5f, -6.5f); // Behind and above
        transposer.m_XDamping = 1.5f;
        transposer.m_YDamping = 1f;
        transposer.m_ZDamping = 1.5f;
        transposer.m_PitchDamping = 1f;
        transposer.m_YawDamping = 0.5f;
        transposer.m_RollDamping = 0.5f;

        // Configure Aim (Composer) for looking at car
        var composer = vcam.GetCinemachineComponent<Unity.Cinemachine.CinemachineComposer>();
        if (composer == null)
        {
            composer = vcam.AddCinemachineComponent<Unity.Cinemachine.CinemachineComposer>();
        }
        composer.m_TrackedObjectOffset = new Vector3(0, 1f, 0); // Look at car center slightly up
        composer.m_LookaheadTime = 0.2f;
        composer.m_LookaheadSmoothing = 3f;
        composer.m_HorizontalDamping = 2f;
        composer.m_VerticalDamping = 2f;
        composer.m_DeadZoneWidth = 0.1f;
        composer.m_DeadZoneHeight = 0.1f;
        composer.m_SoftZoneWidth = 0.3f;
        composer.m_SoftZoneHeight = 0.3f;

        // Add noise for slight camera shake (optional - API varies by version)
        var noise = vcam.GetCinemachineComponent<Unity.Cinemachine.CinemachineBasicMultiChannelPerlin>();
        if (noise == null)
        {
            noise = vcam.AddCinemachineComponent<Unity.Cinemachine.CinemachineBasicMultiChannelPerlin>();
        }
        // Note: Noise properties (AmplitudeGain, FrequencyGain) may be on a NoiseSettings asset
        // in some 2.x versions. Skipping direct property access for compatibility.

        // Position the main camera at a reasonable starting point
        mainCamera.transform.position = car.transform.position + new Vector3(0, 3.5f, -6.5f);
        mainCamera.transform.LookAt(car.transform.position + Vector3.up * 1f);

        // Save scene
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("Cinemachine Chase Camera (2.x) created and configured!");
    }
}