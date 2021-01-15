using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("UPGEN Lighting/UPGEN RayTraced GI")]
[RequireComponent(typeof(Light)), ExecuteInEditMode]
public sealed class UL_RayTracedGI : MonoBehaviour
{
    //-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    [Range(0, 5)] public float intensity = 1;
    [Range(2, 15)] public int raysMatrixSize = 7;
    [Range(0.1f, 10)] public float raysMatrixScale = 1;

    //-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    private const float BOUNCED_LIGHT_RANGE = 8;
    private const float BOUNCED_LIGHT_BOOST = 5;
    private const float SUN_BOUNCED_LIGHT_BOOST = 3;
    private const float SUN_FAR_OFFSET = 100;
    private const float SUN_FAR_OFFSET_DBL = SUN_FAR_OFFSET * 2;

    //-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    public static readonly List<UL_RayTracedGI> all = new List<UL_RayTracedGI>();

    void OnEnable() => all.Add(this);
    void OnDisable() => all.Remove(this);

    //-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    public Light BaseLight => _light == null ? (_light = GetComponent<Light>()) : _light;

    private Light _light;
    private float _lastUpdateTime;
    private float _lastTime;

    internal void GenerateRenderData()
    {
        if (_light == null)
        {
            _light = GetComponent<Light>();
            if (_light == null) return;
        }
        if (!_light.enabled) return;

#if UNITY_EDITOR
        var time = (float)UnityEditor.EditorApplication.timeSinceStartup;
#else
        var time = Time.unscaledTime;
#endif
        var delta = time - _lastTime;
        _lastTime = time;
        if (time - _lastUpdateTime > 0.2f) // Accumulate 5 updates per second
        {
            _lastUpdateTime = time;
            UpdateRaysMatrix();
            if (_rays == UL_Rays.EMPTY_RAYS) return;

            var i = _light.intensity * intensity;
            switch (_light.type) // intensity boost
            {
                case LightType.Point:
                case LightType.Spot: i *= BOUNCED_LIGHT_BOOST / (raysMatrixSize * raysMatrixSize); break;
                case LightType.Directional: i *= SUN_BOUNCED_LIGHT_BOOST; break;
            }
            var c = _light.color.linear * i;
            if (c.maxColorComponent < 0.001f) // light is disabled
            {
                for (var j = _rays.Length - 1; j >= 0; j--) _rays[j].hit = false;
                return;
            }

            var position = transform.position;
            var range = _light.range;
            var layersToRayTrace = UL_Manager.instance ? (int)UL_Manager.instance.layersToRayTrace : -5;

            switch (_light.type)
            {
                case LightType.Directional:
                    var sunRaysStep = raysMatrixSize * raysMatrixScale;
                    var sunRight = transform.right * sunRaysStep;
                    var sunUp = transform.up * sunRaysStep;
                    var sunForward = transform.forward;
                    var sunPosition = transform.position - sunForward * SUN_FAR_OFFSET;
                    for (var j = _rayMatrix2D.Length - 1; j >= 0; j--)
                    {
                        var rj = _rayMatrix2D[j];
                        _rays[j].Trace(sunPosition + rj.x * sunRight + rj.y * sunUp, sunForward, SUN_FAR_OFFSET_DBL, c, layersToRayTrace);
                    }
                    break;

                case LightType.Spot:
                    var spotRadius = Mathf.Tan(Mathf.Deg2Rad * _light.spotAngle * 0.4f) * _light.range;
                    var spotRotation = transform.rotation;
                    var spotForward = transform.forward;
                    for (var j = _rayMatrix2D.Length - 1; j >= 0; j--)
                        _rays[j].Trace(position, (spotForward * range + spotRotation * (_rayMatrix2D[j] * spotRadius)).normalized, range, c, layersToRayTrace);
                    break;

                case LightType.Point:
                    for (var j = _rayMatrix3D.Length - 1; j >= 0; j--)
                        _rays[j].Trace(position, _rayMatrix3D[j], range, c, layersToRayTrace);
                    break;
            }
        }

        // Add lights and interpolate their values
        for (var j = _rays.Length - 1; j >= 0; j--)
        {
            var ray = _rays[j];
            if ((ray.interpolatedPosition - ray.position).sqrMagnitude > 9)
            {
                if (ray.interpolatedColor.maxColorComponent > 0.01f) ray.interpolatedColor = Color.Lerp(ray.interpolatedColor, Color.black, delta * 10f);
                else
                {
                    ray.interpolatedColor = Color.Lerp(ray.interpolatedColor, ray.hit ? ray.color : Color.black, delta * 10f);
                    ray.interpolatedPosition = ray.position;
                }
                UL_Renderer.Add(ray.interpolatedPosition, BOUNCED_LIGHT_RANGE, ray.interpolatedColor);
            }
            else
            {
                ray.interpolatedColor = Color.Lerp(ray.interpolatedColor, ray.hit ? ray.color : Color.black, delta * 5f);
                ray.interpolatedPosition = Vector3.Lerp(ray.interpolatedPosition, ray.position, delta * 10f);
                UL_Renderer.Add(ray.interpolatedPosition, BOUNCED_LIGHT_RANGE, ray.interpolatedColor);
            }
        }
    }

    //-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    private UL_Rays.Ray[] _rays;
    private Vector2[] _rayMatrix2D;
    private Vector3[] _rayMatrix3D;

    private void UpdateRaysMatrix()
    {
        switch (_light.type)
        {
            case LightType.Directional:
            case LightType.Spot:
                var newMatrixSpot = UL_RayMatrices.GRID[raysMatrixSize - 2];
                if (_rayMatrix2D == newMatrixSpot) return;
                _rayMatrix2D = newMatrixSpot;
                _rays = UL_Rays.GenerateRays(newMatrixSpot.Length);
                return;

            case LightType.Point:
                var newMatrixPoint = UL_RayMatrices.SPHERE[raysMatrixSize - 2];
                if (_rayMatrix3D == newMatrixPoint) return;
                _rayMatrix3D = newMatrixPoint;
                _rays = UL_Rays.GenerateRays(newMatrixPoint.Length);
                return;

            default:
                _rays = UL_Rays.EMPTY_RAYS;
                return;
        }
    }

    //-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    private void BuildFastLight(UL_Rays.Ray ray)
    {
        if (!ray.hit) return;

        var go = new GameObject("Fast Light");

        go.transform.SetParent(transform, false);
        go.transform.position = ray.position;

        var cmp = go.AddComponent<UL_FastLight>();
        cmp.intensity = 1;
        cmp.range = BOUNCED_LIGHT_RANGE;
        cmp.color = ray.color.gamma;
#if UNITY_EDITOR
        UnityEditor.Undo.RegisterCreatedObjectUndo(go, "Create Fast Lights");
#endif
    }

    public void CreateFastLights()
    {
        if (!enabled) return;
        GenerateRenderData();
        UL_Renderer.ClearUsedList();

        if (_rays == null) return;
        for (var j = _rays.Length - 1; j >= 0; j--) BuildFastLight(_rays[j]);

#if UNITY_EDITOR
        UnityEditor.Undo.RecordObject(this, "Create Fast Lights");
#endif
        enabled = false;
    }

    //-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    public void DestroyFastLights()
    {
        if (enabled) return;
        for (var i = transform.childCount - 1; i >= 0; i--)
        {
            var tr = transform.GetChild(i);
            if (tr.GetComponent<UL_FastLight>())
            {
#if UNITY_EDITOR
                UnityEditor.Undo.DestroyObjectImmediate(tr.gameObject);
#else
                DestroyImmediate(tr.gameObject);
#endif
            }
        }

#if UNITY_EDITOR
        UnityEditor.Undo.RecordObject(this, "Destroy Fast Lights");
#endif
        enabled = true;
    }

    //-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------
}