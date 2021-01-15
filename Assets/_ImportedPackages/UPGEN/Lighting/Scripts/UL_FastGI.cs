using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("UPGEN Lighting/UPGEN Fast GI")]
[RequireComponent(typeof(Light)), ExecuteInEditMode]
public sealed class UL_FastGI : MonoBehaviour
{
    //-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    [Range(1, 10)] public float expand = 3;
    [Range(0, 1)] public float intensity = 0.1f;

    //-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    public static readonly List<UL_FastGI> all = new List<UL_FastGI>();

    void OnEnable() => all.Add(this);
    void OnDisable() => all.Remove(this);

    //-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    private Light _light;

    internal void GenerateRenderData()
    {
        if (_light == null)
        {
            _light = GetComponent<Light>();
            if (_light == null) return;
        }

        Vector3 pos;
        switch (_light.type)
        {
            case LightType.Spot: pos = transform.position + transform.forward; break;
            case LightType.Point: pos = transform.position; break;
            default: return;
        }

        UL_Renderer.Add(pos, _light.range * expand, _light.intensity * intensity * _light.color.linear);
    }

    //-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------
}