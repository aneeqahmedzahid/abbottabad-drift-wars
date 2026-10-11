using UnityEngine;
using UnityEditor;

public class SetupInputAxes
{
    [MenuItem("Tools/Setup ACP Input Axes")]
    public static void AddInputAxes()
    {
        SerializedObject serializedObject = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/InputManager.asset")[0]);
        SerializedProperty axesProperty = serializedObject.FindProperty("m_Axes");

        // Helper to add an axis if it doesn't exist
        System.Action<string, string, string, string, string, float, float, float, bool> AddAxis = 
            (name, positiveButton, negativeButton, altPositiveButton, altNegativeButton, gravity, dead, sensitivity, snap) => 
        {
            // Check if axis exists
            bool exists = false;
            for (int i = 0; i < axesProperty.arraySize; i++)
            {
                var axis = axesProperty.GetArrayElementAtIndex(i);
                var axisName = axis.FindPropertyRelative("m_Name").stringValue;
                if (axisName == name)
                {
                    exists = true;
                    break;
                }
            }

            if (!exists)
            {
                int index = axesProperty.arraySize;
                axesProperty.InsertArrayElementAtIndex(index);
                var axis = axesProperty.GetArrayElementAtIndex(index);
                axis.FindPropertyRelative("m_Name").stringValue = name;
                axis.FindPropertyRelative("m_PositiveButton").stringValue = positiveButton;
                axis.FindPropertyRelative("m_NegativeButton").stringValue = negativeButton;
                axis.FindPropertyRelative("m_AltPositiveButton").stringValue = altPositiveButton;
                axis.FindPropertyRelative("m_AltNegativeButton").stringValue = altNegativeButton;
                axis.FindPropertyRelative("m_Gravity").floatValue = gravity;
                axis.FindPropertyRelative("m_Dead").floatValue = dead;
                axis.FindPropertyRelative("m_Sensitivity").floatValue = sensitivity;
                axis.FindPropertyRelative("m_Snap").boolValue = snap;
                axis.FindPropertyRelative("m_Type").intValue = 0; // Key or Mouse Button
                axis.FindPropertyRelative("m_Axis").intValue = 0;
                axis.FindPropertyRelative("m_JoyNum").intValue = 0;
                Debug.Log($"Added input axis: {name}");
            }
            else
            {
                Debug.Log($"Input axis already exists: {name}");
            }
        };

        // Add required axes for ACP
        AddAxis("Vertical", "w", "s", "up", "down", 3, 0.001f, 3, true);
        AddAxis("Horizontal", "d", "a", "right", "left", 3, 0.001f, 3, true);
        AddAxis("Brake", "space", "", "", "", 3, 0.001f, 3, false);
        AddAxis("Jump", "space", "", "", "", 3, 0.001f, 3, false);
        AddAxis("Handbrake", "space", "", "", "", 3, 0.001f, 3, false);
        AddAxis("Drift", "left shift", "", "", "", 3, 0.001f, 3, false);
        AddAxis("Boost", "left ctrl", "", "", "", 3, 0.001f, 3, false);

        serializedObject.ApplyModifiedProperties();
        AssetDatabase.Refresh();
        Debug.Log("Input axes setup complete!");
    }
}