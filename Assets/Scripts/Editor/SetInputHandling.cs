using UnityEngine;
using UnityEditor;

public class SetInputHandling
{
    [MenuItem("Tools/Set Input Handling to Both")]
    public static void SetBothInputHandling()
    {
        // Get the PlayerSettings
        var playerSettings = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset")[0];
        SerializedObject serializedObject = new SerializedObject(playerSettings);
        
        // Find the activeInputHandling property
        var activeInputHandling = serializedObject.FindProperty("activeInputHandling");
        
        if (activeInputHandling != null)
        {
            // 0 = Legacy, 1 = Input System, 2 = Both
            activeInputHandling.intValue = 2; // Both
            serializedObject.ApplyModifiedProperties();
            Debug.Log("Active Input Handling set to 'Both' (Legacy + Input System)");
        }
        else
        {
            Debug.LogError("Could not find activeInputHandling property");
        }
    }
}