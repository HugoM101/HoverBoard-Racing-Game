using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hoverboard : MonoBehaviour
{
    public Transform[] hoverPoints;
    public float hoverHeight;
    public float hoverForce;

    public float baseSpeed;
    public float turningSpeed;

    private Rigidbody rb;


    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Move(float moveInput, float turnInput)
    {
        Vector3 moveDirection = transform.forward * moveInput * baseSpeed;
        rb.AddForce(moveDirection, ForceMode.Acceleration);

        float turnAmount = turnInput * turningSpeed;
        rb.AddTorque(transform.up * turnAmount, ForceMode.Acceleration);

    }
}
