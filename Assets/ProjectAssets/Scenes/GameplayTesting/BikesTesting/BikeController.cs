using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BikeController : MonoBehaviour
{
    [Header("Forward movement")]
    [SerializeField] float _speed;
    [SerializeField] float _acceleration;
    [Header("Rotation")]
    [SerializeField] float _angleSpeed;
    [Header("Traceback")]
    [SerializeField] BikeTraceback _traceback;
    private Rigidbody body;
    private float wheelAngle = 0;
    private float maxWheelAngle = 70f;
    private void Awake()
    {
        body = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        float horizontalAxis = Input.GetAxis("Horizontal");
        if(body.velocity.magnitude < _speed)
            body.AddForce(transform.forward * _acceleration, ForceMode.Acceleration);
        transform.Rotate(transform.up, horizontalAxis * _angleSpeed * Time.deltaTime);
    }
}
