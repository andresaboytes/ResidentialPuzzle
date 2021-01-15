using UnityEngine;

[AddComponentMenu("UPGEN Lighting/UPGEN Lighting Manager")]
[ExecuteInEditMode]
public sealed class UL_Manager : MonoBehaviour
{
    //-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    public static UL_Manager instance;

    void OnEnable()
    {
        if (instance == null) instance = this;
        else Debug.LogWarning("There are 2 audio UPGEN Lighting Managers in the scene. Please ensure there is always exactly one Manager in the scene.");
    }

    void OnDisable()
    {
        if (instance == this) instance = null;
    }

    //-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    public LayerMask layersToRayTrace = -5;
    public bool showDebugRays;

    //-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    public bool showDebugGUI = true;

    void OnGUI()
    {
        if (!showDebugGUI) return;

        GUILayout.BeginArea(new Rect(0, 0, 200, Screen.height));
        {
            if (UL_Renderer.HasLightsToRender)
            {
                var cnt = UL_Renderer.RenderedLightsCount;
                if (cnt > 0) UL_GUI_Utils.Text($"Capacity: <b>{cnt} / {UL_Renderer.MaxRenderingLightsCount}</b>");

                cnt = UL_FastLight.all.Count;
                if (cnt > 0) UL_GUI_Utils.Text($"Fast Lights: <b>{cnt}</b>");

                cnt = UL_FastGI.all.Count;
                if (cnt > 0) UL_GUI_Utils.Text($"Fast GI: <b>{cnt}</b>");

                cnt = UL_RayTracedGI.all.Count;
                if (cnt > 0) UL_GUI_Utils.Text($"RayTraced GI: <b>{cnt}</b>");
            }
        }
        GUILayout.EndArea();
    }

    //-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------
}