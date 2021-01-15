using UnityEditor;

[CustomEditor(typeof(UL_FastGI)), CanEditMultipleObjects]
public class eUL_FastGI : Editor
{
    //-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(UL_FastGI.expand)));
        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(UL_FastGI.intensity)));

        serializedObject.ApplyModifiedProperties();
    }

    //-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------
}
