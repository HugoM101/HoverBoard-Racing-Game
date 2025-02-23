using UnityEngine;
using UnityEngine.Splines;

public class AI : MonoBehaviour
{
    public SplineContainer splineContainer;

    public float baseSpeed; 
    public float rotationSharpness; 
    public Vector3 orientationOffset = Vector3.zero; 

    public Transform[] hoverPoints; 
    public float hoverHeight; 
    public float hoverForce; 
    public float maxHoverDistance; 

    public float startProgress;

    private float progress = 0f; 
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        //***setting start point and rotation for testing. same as the move code. to be removed later***
        progress = startProgress;

        Vector3 initialPosition = splineContainer.EvaluatePosition(progress);
        transform.position = initialPosition;

        Vector3 initialTangent = splineContainer.EvaluateTangent(progress);
        initialTangent = initialTangent.normalized;

        Vector3 initialUp = splineContainer.EvaluateUpVector(progress);

        Quaternion initialRotation = Quaternion.LookRotation(initialTangent, initialUp);
        Quaternion offsetRotation = Quaternion.Euler(orientationOffset);
        transform.rotation = initialRotation * offsetRotation;
    }

    void FixedUpdate() 
    {
        Move();
        Hover(); 
    }

    void Move()
    {
        //move along the spline
        progress += baseSpeed * Time.deltaTime / splineContainer.CalculateLength();
        progress = Mathf.Repeat(progress, 1f);

        //set pos
        Vector3 targetPosition = splineContainer.EvaluatePosition(progress);
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * baseSpeed);

        //*****set rot******
        //get tangent (direction of travel)
        Vector3 targetTangent = splineContainer.EvaluateTangent(progress);
        targetTangent = targetTangent.normalized;

        Vector3 up = splineContainer.EvaluateUpVector(progress);

        /*creates a rotation so that the ai's forward direction lines up with the splines tangent 
        and the up direction aligns with splines up vector*/
        Quaternion targetRotation = Quaternion.LookRotation(targetTangent, up);
        
        //apply offset set to change direction board is facing
        Quaternion offsetRotation = Quaternion.Euler(orientationOffset);
        targetRotation = targetRotation * offsetRotation;

        //smoothly rotate the ai
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSharpness); //more sharpness = quicker visual turning
    }

    //same physics as player hoverboard script
    void Hover()
    {
        foreach (Transform point in hoverPoints)
        {
            RaycastHit hit;
            if (Physics.Raycast(point.position, -transform.up, out hit, maxHoverDistance))
            {
                Debug.DrawRay(point.position, -transform.up * hit.distance, Color.green);

                float distance = hoverHeight - hit.distance;
                float force = distance * hoverForce;
                
                Vector3 hoverForceDirection = hit.normal * force;
                rb.AddForceAtPosition(hoverForceDirection, point.position, ForceMode.Acceleration);
            }
            else
            {
                rb.AddForce(Vector3.down * hoverForce, ForceMode.Acceleration);
            }
        }
    }
}