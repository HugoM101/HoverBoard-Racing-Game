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

    
    public Transform player;
    public float minSpeedModifier; 
    public float maxSpeedModifier; 
    public float rubberBandStrength; 
    private float adjustedSpeed;

    public float lateralOffset;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        PositionTracker.Instance.RegisterRacer(transform, false, startProgress); //register ai racers to positiontracker

        progress = startProgress;

        Vector3 initialPosition = splineContainer.EvaluatePosition(progress);
   
        Vector3 initialTangent = splineContainer.EvaluateTangent(progress);
        initialTangent = initialTangent.normalized;

        Vector3 initialUp = splineContainer.EvaluateUpVector(progress);

        Vector3 right = Vector3.Cross(initialUp, initialTangent).normalized;
        initialPosition += right * lateralOffset;

        initialPosition += initialUp * hoverHeight;

        transform.position = initialPosition;

        Quaternion initialRotation = Quaternion.LookRotation(initialTangent, initialUp);
        Quaternion offsetRotation = Quaternion.Euler(orientationOffset);
        transform.rotation = initialRotation * offsetRotation;

        adjustedSpeed = baseSpeed;
    }

    void FixedUpdate() 
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameStarted)
        {
            RubberBanding();
            Move();
            Hover(); 
            ObstacleAvoidance();
        }

        else
        {
            Hover(); 
        }
    }

    void Move()
    {
        //move along the spline
        progress += adjustedSpeed * Time.deltaTime / splineContainer.CalculateLength();
        progress = Mathf.Repeat(progress, 1f);

        //*****set pos******
        Vector3 targetPosition = splineContainer.EvaluatePosition(progress);
        Vector3 up = splineContainer.EvaluateUpVector(progress);

        //get tangent (direction of travel)
        Vector3 targetTangent = splineContainer.EvaluateTangent(progress);
        targetTangent = targetTangent.normalized;
        Vector3 right = Vector3.Cross(up, targetTangent).normalized;
        targetPosition += right * lateralOffset;//to allow for the spacing out of each ai
        targetPosition += avoidanceOffset; //to apply offset for avoiding obstacles
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * adjustedSpeed);
   
        //*****set rot******
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

    void RubberBanding()
    {
        //get players approx progress along the spline
        float playerProgress = 0f;
        foreach (var racer in PositionTracker.Instance.racers)
        {
            if (racer.isPlayer)
            {
                playerProgress = racer.progress;
                break;
            }
        }

        //calculate the distance between ai and player
        float progressDifference = progress - playerProgress;

        float speedModifier = 1f;

        if (progressDifference < 0) //ai behind the player
        {
            speedModifier = Mathf.Lerp(1f, maxSpeedModifier, Mathf.Abs(progressDifference) * rubberBandStrength);
        }

        else if (progressDifference > 0) //ai ahead of the player
        {
            speedModifier = Mathf.Lerp(1f, minSpeedModifier, progressDifference * rubberBandStrength);
        }

        adjustedSpeed = baseSpeed * speedModifier;
    }
}