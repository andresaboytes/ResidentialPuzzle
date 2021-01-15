using System.Collections.Generic;
using UnityEngine;

public static class UL_Renderer
{
    //-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    public static bool HasLightsToRender => UL_FastLight.all.Count > 0 || UL_FastGI.all.Count > 0 || UL_RayTracedGI.all.Count > 0;
    public static int RenderedLightsCount => _lightsCount;
    public static int MaxRenderingLightsCount => MAX_LIGHTS_COUNT;

    //-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    private const int MAX_LIGHTS_COUNT = 96;
    private static int _lightsCount;
    private static readonly Vector4[] _lightsPositions = new Vector4[MAX_LIGHTS_COUNT];
    private static readonly Vector4[] _lightsColors = new Vector4[MAX_LIGHTS_COUNT];

    //-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    private class Light
    {
        public float score;
        public Vector4 position;
        public Vector4 color;
    }

    private static readonly Stack<Light> _lightsPool = new Stack<Light>();
    private static readonly List<Light> _lightsUsed = new List<Light>();

    private static int ScoresComparison(Light x, Light y)
    {
        if (x.score < y.score) return -1;
        if (x.score > y.score) return 1;
        return 0;
    }

    //-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    private static Vector3 _cameraPosition;
    private static Vector3 _cameraForward;

    private const float _cameraFOVAngle = 50; // deg
    private static readonly float _cameraFOVCos = Mathf.Cos(_cameraFOVAngle * Mathf.Deg2Rad);
    private static readonly float _cameraFOVSin = Mathf.Sin(_cameraFOVAngle * Mathf.Deg2Rad);

    public static void Add(Vector3 position, float range, Color color)
    {
        if (color.maxColorComponent < 0.001f) return; // ignore too dark lights

        var v = position - _cameraPosition;
        var dot = Vector3.Dot(v, _cameraForward);
        var x = _cameraFOVCos * Mathf.Sqrt(Vector3.Dot(v, v) - dot * dot) - dot * _cameraFOVSin;
        if (x >= 0 && Mathf.Abs(x) >= range) return; // cull it as light is out of camera cone

        var lt = _lightsPool.Count > 0 ? _lightsPool.Pop() : new Light();
        lt.score = v.sqrMagnitude - (2 - dot) * range; // smaller are better
        lt.position.x = position.x;
        lt.position.y = position.y;
        lt.position.z = position.z;
        lt.position.w = range;
        lt.color.x = color.r;
        lt.color.y = color.g;
        lt.color.z = color.b;
        _lightsUsed.Add(lt);
    }

    public static void ClearUsedList()
    {
        for (var i = _lightsUsed.Count - 1; i >= _lightsCount; i--) _lightsPool.Push(_lightsUsed[i]);
        _lightsUsed.Clear();
    }
    
    //-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    public static void SetupForCamera(Camera camera, MaterialPropertyBlock properties)
    {
        // 1. Cache camera data
        var cameraTransform = camera.transform;
        _cameraPosition = cameraTransform.position;
        _cameraForward = cameraTransform.forward;

        // 2. Generate lights data
        for (var i = UL_FastLight.all.Count - 1; i >= 0; i--) UL_FastLight.all[i].GenerateRenderData();
        for (var i = UL_FastGI.all.Count - 1; i >= 0; i--) UL_FastGI.all[i].GenerateRenderData();
        for (var i = UL_RayTracedGI.all.Count - 1; i >= 0; i--) UL_RayTracedGI.all[i].GenerateRenderData();

        // 3. Transfer lights data to arrays
        _lightsCount = Mathf.Min(_lightsUsed.Count, MAX_LIGHTS_COUNT);
        _lightsUsed.Sort(ScoresComparison);
        for (var i = _lightsCount - 1; i >= 0; i--)
        {
            var ld = _lightsUsed[i];
            _lightsPositions[i] = ld.position;
            _lightsColors[i] = ld.color;
            _lightsPool.Push(ld);
        }
        ClearUsedList();
        properties.SetInt("_LightsCount", _lightsCount);
        properties.SetVectorArray("_LightsPositions", _lightsPositions);
        properties.SetVectorArray("_LightsColors", _lightsColors);

        // 4. Setup material matrices
        if (camera.stereoEnabled)
        {
            var leftViewFromScreen = GL.GetGPUProjectionMatrix(camera.GetStereoProjectionMatrix(Camera.StereoscopicEye.Left), true).inverse;
            leftViewFromScreen[1, 1] *= -1;
            properties.SetMatrix("_LeftViewFromScreen", leftViewFromScreen);
            properties.SetMatrix("_LeftWorldFromView", camera.GetStereoViewMatrix(Camera.StereoscopicEye.Left).inverse);

            var rightViewFromScreen = GL.GetGPUProjectionMatrix(camera.GetStereoProjectionMatrix(Camera.StereoscopicEye.Right), true).inverse;
            rightViewFromScreen[1, 1] *= -1;
            properties.SetMatrix("_RightViewFromScreen", rightViewFromScreen);
            properties.SetMatrix("_RightWorldFromView", camera.GetStereoViewMatrix(Camera.StereoscopicEye.Right).inverse);
        }
        else
        {
            var leftViewFromScreen = GL.GetGPUProjectionMatrix(camera.projectionMatrix, true).inverse;
            leftViewFromScreen[1, 1] *= -1;
            properties.SetMatrix("_LeftViewFromScreen", leftViewFromScreen);
            properties.SetMatrix("_LeftWorldFromView", camera.cameraToWorldMatrix);
        }
    }

    //-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------
}