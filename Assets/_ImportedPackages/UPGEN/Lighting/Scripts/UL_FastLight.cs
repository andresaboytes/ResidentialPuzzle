using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("UPGEN Lighting/UPGEN Fast Light")]
[ExecuteInEditMode]
public sealed class UL_FastLight : MonoBehaviour
{
    //-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    public Color color = Color.white;
    public float intensity = 1;
    public float range = 10;

    //-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    public static readonly List<UL_FastLight> all = new List<UL_FastLight>();

    void OnEnable() => all.Add(this);
    void OnDisable() => all.Remove(this);

    //-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    internal void GenerateRenderData() => UL_Renderer.Add(transform.position, range, intensity * intensity * color.linear);

    //-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------
}