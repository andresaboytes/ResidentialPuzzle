using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(UL_RayTracedGI)), CanEditMultipleObjects]
public class eUL_RayTracedGI : Editor
{
    //-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    public override void OnInspectorGUI()
    {
        var rayTracedGI = (UL_RayTracedGI)target;
        serializedObject.Update();

        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(UL_RayTracedGI.intensity)));
        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(UL_RayTracedGI.raysMatrixSize)));

        if (rayTracedGI.BaseLight?.type == LightType.Directional)
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(UL_RayTracedGI.raysMatrixScale)));

        if (rayTracedGI.enabled)
        { if (GUILayout.Button("Generate Fast Lights")) rayTracedGI.CreateFastLights(); }
        else if (GUILayout.Button("Destroy Fast Lights")) rayTracedGI.DestroyFastLights();

        serializedObject.ApplyModifiedProperties();
    }

    //-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------
}
