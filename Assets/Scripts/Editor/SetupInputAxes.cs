using UnityEngine;
using UnityEditor;

public class SetupInputAxes
{
    [MenuItem("Tools/Setup ACP Input Axes")]
    public static void AddInputAxes()
    {
        var inputManager = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/InputManager.asset");
        if (inputManager.Length == 0)
        {
            Debug.LogError("Could not find InputManager.asset");
            return;
        }

        SerializedObject serializedObject = new SerializedObject(inputManager[0]);
        SerializedProperty axesProperty = serializedObject.FindProperty("m_Axes");

        if (axesProperty == null)
        {
            Debug.LogError("Could not find m_Axes property in InputManager");
            return;
        }

        // Helper to add an axis if it doesn't exist
        System.Action<string, string, string, string, string, float, float, float, bool> AddAxis = 
            (name, positiveButton, negativeButton, altPositiveButton, altNegativeButton, gravity, dead, sensitivity, snap) => 
        {
            // Check if axis exists
            bool exists = false;
            for (int i = 0; i < axesProperty.arraySize; i++)
            {
                var axis = axesProperty.GetArrayElementAtIndex(i);
                var axisNameProp = axis.FindPropertyRelative("m_Name");
                if (axisNameProp != null && axisNameProp.stringValue == name)
                {
                    exists = true;
                    break;
                }
            }

            if (!exists)
            {
                int index = axesProperty.arraySize;
                axesProperty.InsertArrayElementAtIndex(index);
                
                // Force update to ensure the new element is properly initialized
                serializedObject.ApplyModifiedProperties();
                serializedObject.Update();
                
                var axis = axesProperty.GetArrayElementAtIndex(index);
                
                var nameProp = axis.FindPropertyRelative("m_Name");
                var posBtnProp = axis.FindPropertyRelative("m_PositiveButton");
                var negBtnProp = axis.FindPropertyRelative("m_NegativeButton");
                var altPosBtnProp = axis.FindPropertyRelative("m_AltPositiveButton");
                var altNegBtnProp = axis.FindPropertyRelative("m_AltNegativeButton");
                var gravityProp = axis.FindPropertyRelative("m_Gravity");
                var deadProp = axis.FindPropertyRelative("m_Dead");
                var sensitivityProp = axis.FindPropertyRelative("m_Sensitivity");
                var snapProp = axis.FindPropertyRelative("m_Snap");
                var typeProp = axis.FindPropertyRelative("m_Type");
                var axisProp = axis.FindPropertyRelative("m_Axis");
                var joyNumProp = axis.FindPropertyRelative("m_JoyNum");

                if (nameProp != null) nameProp.stringValue = name;
                if (posBtnProp != null) posBtnProp.stringValue = positiveButton;
                if (negBtnProp != null) negBtnProp.stringValue = negativeButton;
                if (altPosBtnProp != null) altPosBtnProp.stringValue = altPositiveButton;
                if (altNegBtnProp != null) altNegBtnProp.stringValue = altNegativeButton;
                if (gravityProp != null) gravityProp.floatValue = gravity;
                if (deadProp != null) deadProp.floatValue = dead;
                if (sensitivityProp != null) sensitivityProp.floatValue = sensitivity;
                if (snapProp != null) snapProp.boolValue = snap;
                if (typeProp != null) typeProp.intValue = 0;
                if (axisProp != null) axisProp.intValue = 0;
                if (joyNumProp != null) joyNumProp.intValue = 0;

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