using UnityEngine;
using UnityEngine.Splines;

public class AI : MonoBehaviour
{   
    [Header("Position, Movement and Rotation Settings")]
    public float startProgress;
    public float lateralOffset;
    public Vector3 orientationOffset; 
    public float baseSpeed; 
    public float rotationSharpness; 

    [Header("Hovering Settings")]
    public Transform[] hoverPoints; 
    public float hoverHeight; 
    public float hoverForce; 
    public float maxHoverDistance; 

    [Header("Obstacle Avoidance")]
    public float obstacleAvoidanceDistance;
    private Vector3 avoidanceOffset;
    public float avoidanceStrength;
    public LayerMask obstacleLayer;

    [Header("Rubberbanding")] 
    public float minSpeedModifier; 
    public float maxSpeedModifier; 
    public float rubberBandStrength; 

    [Header("Spline")] 
    public SplineContainer splineContainer;
    private Vector3 targetPosition;
    private Vector3 up;
    private Vector3 targetTangent;
    private Vector3 right;
    private Quaternion targetRotation;
    private Quaternion offsetRotation;

    private float progress = 0f; 
    private Rigidbody rb;
    private float adjustedSpeed;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        //registers this ai to the position tracker
        PositionTracker.Instance.RegisterRacer(transform, false, startProgress); 

        progress = startProgress;

        #region Start position + Rotation

        /********set position*********
        ******************************
        */

        targetPosition = splineContainer.EvaluatePosition(progress);

        //get up
        up = splineContainer.EvaluateUpVector(progress);
   
        //get tangent (direction of travel) + calculate right
        targetTangent = splineContainer.EvaluateTangent(progress);
        targetTangent = targetTangent.normalized;
        right = Vector3.Cross(up, targetTangent).normalized;

        targetPosition += right * lateralOffset; //to allow for the spacing out of each ai
        targetPosition += up * hoverHeight;

        //set position
        transform.position = targetPosition;

        /*********set rot***********
        ****************************
        */
        targetRotation = Quaternion.LookRotation(targetTangent, up);
        offsetRotation = Quaternion.Euler(orientationOffset);
        transform.rotation = targetRotation * offsetRotation;
        #endregion

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

    #region Movement
    void Move()
    {
        //move along the spline
        progress += adjustedSpeed * Time.deltaTime / splineContainer.CalculateLength();
        progress = Mathf.Repeat(progress, 1f);


        //Similar to the start function code

        /********set position*********
        ******************************
        */

        targetPosition = splineContainer.EvaluatePosition(progress);

        //get up
        up = splineContainer.EvaluateUpVector(progress);

        //get tangent (direction of travel) + calculate right
        targetTangent = splineContainer.EvaluateTangent(progress);
        targetTangent = targetTangent.normalized;
        right = Vector3.Cross(up, targetTangent).normalized;

        targetPosition += right * lateralOffset;//to allow for the spacing out of each ai
        targetPosition += avoidanceOffset; //to apply offset for avoiding obstacles
        
        //lerp to target pos
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * adjustedSpeed);
   
        /*********set rot***********
        ****************************
        */
        /*creates a rotation so that the ai's forward direction lines up with the splines tangent 
        and the up direction aligns with splines up vector*/
        targetRotation = Quaternion.LookRotation(targetTangent, up);
        
        //apply offset set to change direction board is facing
        offsetRotation = Quaternion.Euler(orientationOffset);
        targetRotation = targetRotation * offsetRotation;

        //smoothly rotate the ai
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSharpness); //more sharpness = quicker visual turning
    }
    #endregion

    #region Hovering
    //same physics as hoverboard script
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
                rb.AddForce(Vector3.down * 4.0f, ForceMode.Acceleration);
            }
        }
    }
    #endregion

    #region AI Features
    private void ObstacleAvoidance()
    {
        RaycastHit hitFront, hitLeft, hitRight, hitLeft45, hitRight45;

        /*the axis of hoverboard asset model was conflicting with unity
         so i trial and errored to reassign vectors
        */
        Vector3 forward = -transform.right; //negative X
        Vector3 left = -transform.forward; //negative Z
        Vector3 right = transform.forward; //positive Z
        Vector3 left45 = (forward + left).normalized; //45 degree left
        Vector3 right45 = (forward + right).normalized; //45 degree right

                                                                                    /*angled 45 degree rays are longer to 
                                                                                    allow more time to detect obstacles at angles*/
        bool obstacleAhead = Physics.Raycast(transform.position, forward, out hitFront, obstacleAvoidanceDistance, obstacleLayer);
        bool obstacleLeft = Physics.Raycast(transform.position, left, out hitLeft, obstacleAvoidanceDistance, obstacleLayer);
        bool obstacleRight = Physics.Raycast(transform.position, right, out hitRight, obstacleAvoidanceDistance, obstacleLayer);
        bool obstacleLeft45 = Physics.Raycast(transform.position, left45, out hitLeft45, obstacleAvoidanceDistance *2, obstacleLayer);
        bool obstacleRight45 = Physics.Raycast(transform.position, right45, out hitRight45, obstacleAvoidanceDistance*2, obstacleLayer);

        Debug.DrawRay(transform.position, forward * obstacleAvoidanceDistance, Color.cyan);
        Debug.DrawRay(transform.position, left * obstacleAvoidanceDistance, Color.blue);
        Debug.DrawRay(transform.position, right * obstacleAvoidanceDistance, Color.red);
        Debug.DrawRay(transform.position, left45 * obstacleAvoidanceDistance*2, Color.magenta);
        Debug.DrawRay(transform.position, right45 * obstacleAvoidanceDistance*2, Color.yellow);

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
        //get player + ai approx progress along the spline
        float playerProgress = 0f;
        float aiProgress = 0f;
        
        foreach (var racer in PositionTracker.Instance.racers)
        {
            if (racer.isPlayer)
            {
                playerProgress = racer.progress;
                
            }
            
            if (racer.racerTransform == transform)
            {
                aiProgress = racer.progress;
            }
        }

        //calculate the distance between ai and player
        float progressDifference = aiProgress - playerProgress;

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
    #endregion
}