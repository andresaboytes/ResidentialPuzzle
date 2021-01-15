using UnityEditor;

[CustomEditor(typeof(UL_Manager)), CanEditMultipleObjects]
public class eUL_Manager : Editor
{
    //-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(UL_Manager.layersToRayTrace)));
        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(UL_Manager.showDebugRays)));
        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(UL_Manager.showDebugGUI)));

        serializedObject.ApplyModifiedProperties();
    }

    //-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------
}
