using UnityEditor;

[CustomEditor(typeof(UL_FastLight)), CanEditMultipleObjects]
public class eUL_FastLight : Editor
{
    //-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(UL_FastLight.range)));
        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(UL_FastLight.color)));
        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(UL_FastLight.intensity)));

        serializedObject.ApplyModifiedProperties();
    }

    //-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------
}
