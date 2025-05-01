using System.Collections;
using UnityEngine;

public class Hoverboard : MonoBehaviour
{
    [Header("Hovering Settings")]
    public Transform[] hoverPoints; //raycasts are emitted from these points
    public float hoverHeight; //height from surface
    public float hoverForce; //force to maintain the hovering
    public float maxHoverDistance; //maximum distance able to detect surfaces
    private float startForce; 
    private bool isHovering;
    private float savedHoverForce; //stores the hoverforce val set in the inspector

    [Header("Speed Settings")]
    public float baseSpeed;
    public float turningSpeed;

    [Header("Boost Settings")]
    public float boostSpeed;
    public float boostDuration;
    public float boostCooldown;
    private bool isBoosting;
    public bool IsBoosting { get { return isBoosting; } }
    private bool canBoost;

    [Header("References")] 
    public UIManager uiManager;

    private LayerMask obstacleLayer;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        startForce = 10.0f;
        isHovering = false;
        isBoosting = false;
        canBoost = true;
        savedHoverForce = hoverForce;

        StartCoroutine(IncreaseHoverForceOverTime());
        
        //this will ignore objects on obstacle layer to fix the bug described in weeks 5-6
        obstacleLayer = ~LayerMask.GetMask("Obstacle");

        //this will initialise the boost ui with being available and not currently being used
        uiManager.UpdateBoostStatus(true, false);
    }

    //using fixed update used as we are applying forces and torques
    void FixedUpdate()
    {
        if(isHovering == true)
        {
            Hover();
        }
    }

    #region Movement
    //to move and turn the hoverboard
    public void Move(float moveInput, float turnInput)
    {
        float currentSpeed;

        if (isBoosting)
        {
            currentSpeed = boostSpeed;
        }

        else
        {
            currentSpeed = baseSpeed;
        }
        
        Vector3 moveDirection = transform.right * moveInput * currentSpeed;
        rb.AddForce(moveDirection, ForceMode.Acceleration);

        float turnAmount = turnInput * turningSpeed;
        rb.AddTorque(transform.up * turnAmount, ForceMode.Acceleration);
    }
    #endregion

    #region Hovering
    void Hover()
    {
        foreach (Transform point in hoverPoints)
        {
            RaycastHit hit;
            
            //casts a ray down to check if a surface is hit
            if (Physics.Raycast(point.position, -transform.up, out hit, maxHoverDistance, obstacleLayer))
            {
                Debug.DrawRay(point.position, -transform.up * hit.distance, Color.green);

                //calculates the diff between the hover height set and the actual distance from origin to impact point
                float distance = hoverHeight - hit.distance;

                //the force needed to actually maintain the hovering height
                float force = distance * hoverForce;


                //apply the force upwards in the direction of surface normal
                Vector3 hoverForceDirection = hit.normal * force;
                rb.AddForceAtPosition(hoverForceDirection, point.position, ForceMode.Acceleration);
            }

            else
            {
                //if there is no ground detected then i apply a light downward force so falling is smooth.
                rb.AddForce(Vector3.down * 4.0f, ForceMode.Acceleration);
            }
        }
    }

    //for the purpose of stable spawn and position resetting
    private IEnumerator IncreaseHoverForceOverTime()
    {
        //wait for 0.5 to allow reset/spawn to settle
        yield return new WaitForSeconds(0.5f);

        //set to startForce and enable hovering
        hoverForce = startForce;
        isHovering = true;

        //start the lerp to normal hover force which was set in inspector
        float elapsedTime = 0f;
        float increaseDuration = 2.0f; //

        while (elapsedTime < increaseDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / increaseDuration;
            hoverForce = Mathf.Lerp(startForce, savedHoverForce, t);
            yield return null;
        }
    }

    //this function will instantly reset position and rotation. 
    public void ResetHoverboard(Vector3 position, Quaternion rotation)
    {
        transform.position = position;
        transform.rotation = rotation;

        isHovering = false; //stop hovering for stable reset
    
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        StartCoroutine(IncreaseHoverForceOverTime());
    }
    #endregion

    #region Boost
    public void Boost()
    {
        if (canBoost)
        {
            StartCoroutine(BoostRoutine());
        }
    }

    private IEnumerator BoostRoutine()
    {
        isBoosting = true;
        canBoost = false;
        uiManager.UpdateBoostStatus(false, true);
        //start boost

        yield return new WaitForSeconds(boostDuration);
        isBoosting = false;
        uiManager.UpdateBoostStatus(false, false, boostCooldown);
        //boost end

        yield return new WaitForSeconds(boostCooldown);
        canBoost = true;
        uiManager.UpdateBoostStatus(true, false);
        //boost available again
    }

    #endregion
}