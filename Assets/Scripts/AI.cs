using UnityEngine;
using UnityEngine.Splines;

public class AI : MonoBehaviour
{
    public SplineContainer splineContainer;

    public float baseSpeed; 
    public float rotationSharpness; 
    public Vector3 orientationOffset; 

    public Transform[] hoverPoints; 
    public float hoverHeight; 
    public float hoverForce; 
    public float maxHoverDistance; 

    public float startProgress;

    private float progress = 0f; 
    private Rigidbody rb;

    public float obstacleAvoidanceDistance;
    private Vector3 avoidanceOffset;
    public float avoidanceStrength;
    public LayerMask obstacleLayer; 

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
        ObstacleAvoidance();
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

        //to apply offset for avoiding obstacles
        targetPosition += avoidanceOffset;

        Vector3 up = splineContainer.EvaluateUpVector(progress);

        /*creates a rotation so that the ai's forward direction lines up with the splines tangent 
        and the up direction aligns with splines up vector*/
        Quaternion targetRotation = Quaternion.LookRotation(targetTangent, up);
        
        //apply offset set to change direction board is facing
        Quaternion offsetRotation = Quaternion.Euler(orientationOffset);
        targetRotation = targetRotation * offsetRotation;

        //smoothly rotate the ai
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSharpness); //more sharpness = quicker visual turning

        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * baseSpeed);
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

    private void ObstacleAvoidance()
    {
        RaycastHit hitFront, hitLeft, hitRight, hitLeft45, hitRight45;

        //the axis of hoverboard asset model was conflictiing with unity
        Vector3 forward = -transform.right;             //negative X
        Vector3 left = -transform.forward;              //negative Z
        Vector3 right = transform.forward;              //positive Z
        Vector3 left45 = (forward + left).normalized;   //45 degree left
        Vector3 right45 = (forward + right).normalized; //45 degree right

                                                                                    /*forward avoidance distance 
                                                                                    shorter to allow the angled ones to 
                                                                                    have more time to detect obstacles*/
        bool obstacleAhead = Physics.Raycast(transform.position, forward, out hitFront, obstacleAvoidanceDistance / 2f, obstacleLayer);
        bool obstacleLeft = Physics.Raycast(transform.position, left, out hitLeft, obstacleAvoidanceDistance, obstacleLayer);
        bool obstacleRight = Physics.Raycast(transform.position, right, out hitRight, obstacleAvoidanceDistance, obstacleLayer);
        bool obstacleLeft45 = Physics.Raycast(transform.position, left45, out hitLeft45, obstacleAvoidanceDistance, obstacleLayer);
        bool obstacleRight45 = Physics.Raycast(transform.position, right45, out hitRight45, obstacleAvoidanceDistance, obstacleLayer);

        Debug.DrawRay(transform.position, forward * obstacleAvoidanceDistance / 2f, Color.cyan);
        Debug.DrawRay(transform.position, left * obstacleAvoidanceDistance, Color.blue);
        Debug.DrawRay(transform.position, right * obstacleAvoidanceDistance, Color.red);
        Debug.DrawRay(transform.position, left45 * obstacleAvoidanceDistance, Color.magenta);
        Debug.DrawRay(transform.position, right45 * obstacleAvoidanceDistance, Color.yellow);

        if (obstacleAhead)
        {
            if (!obstacleRight && !obstacleRight45)
            {
                avoidanceOffset = right * avoidanceStrength;
            }

            else if (!obstacleLeft && !obstacleLeft45)
            {
                avoidanceOffset = left * avoidanceStrength;
            }

            else if (!obstacleRight)
            {
                avoidanceOffset = right * avoidanceStrength;
            }

            else if (!obstacleLeft)
            {
                avoidanceOffset = left * avoidanceStrength;
            }
            
        }

        else
        {
            //reset offset back to 0 if nothing detected
            avoidanceOffset = Vector3.Lerp(avoidanceOffset, Vector3.zero, Time.deltaTime * 2f);
        }
    }
}