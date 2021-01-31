using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BikeTraceback : MonoBehaviour
{
    [SerializeField] Transform _tracebackOrigin;
    [SerializeField] GameObject _tracebackObject;
    [SerializeField] Vector3 _tracebackDetectorSize;
    [SerializeField] LayerMask _traceLayer;

    private List<GameObject> tracebackElements = new List<GameObject>();

    private void OnDrawGizmosSelected()
    {
        if (_tracebackOrigin)
            Gizmos.DrawCube(_tracebackOrigin.position, _tracebackDetectorSize);
    }

    private void Update()
    {
        if (Physics.OverlapBox(_tracebackOrigin.position, _tracebackDetectorSize,Quaternion.LookRotation(_tracebackOrigin.forward,Vector3.up),_traceLayer).Length == 0)
        {
            GameObject trace = Instantiate(_tracebackObject, _tracebackOrigin.position, Quaternion.LookRotation(_tracebackOrigin.forward, Vector3.up));
            tracebackElements.Add(trace);
        }
    }

    public void Clear()
    {
        for (int i = 0; i < tracebackElements.Count; i++)
        {
            Destroy(tracebackElements[i]);
        }
        tracebackElements.Clear();
    }
}
