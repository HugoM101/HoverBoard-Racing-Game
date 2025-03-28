using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hoverboard : MonoBehaviour
{
    public Transform[] hoverPoints;
    public float hoverHeight;
    public float hoverForce;
    public float maxHoverDistance; //maximum distance able to detect surfaces
    private float startForce = 10.0f;
    private bool hovering = false;

    private float savedHoverForce;

    public float baseSpeed;
    public float turningSpeed;

    public float boostSpeed;
    public float boostDuration = 4f;
    public float boostCooldown = 15f;
    private bool isBoosting = false;
    public bool IsBoosting { get { return isBoosting; } }
    private bool canBoost = true;

    private LayerMask obstacleLayerMask;

    public UIManager uiManager;

    private Rigidbody rb;


    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        savedHoverForce = hoverForce;
        StartCoroutine(IncreaseHoverForceOverTime());

        obstacleLayerMask = ~LayerMask.GetMask("Obstacle");

        uiManager.UpdateBoostStatus(true, false);
    }

    //fixed update used as we are applying forces and torques
    void FixedUpdate()
    {
        if(hovering == true)
        {
            Hover();
        }
    }

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

    //handles the hovering physics of the board
    void Hover()
    {
        foreach (Transform point in hoverPoints)
        {
            RaycastHit hit;
            
            //casts a ray down to check if a surface is hit
            if (Physics.Raycast(point.position, -transform.up, out hit, maxHoverDistance, obstacleLayerMask))
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
                //if there is no ground detected then apply downward force 
                rb.AddForce(Vector3.down * 5.0f, ForceMode.Acceleration);
            }
        }
    }

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

    //for the purpose of stable spawn and position resetting
    private IEnumerator IncreaseHoverForceOverTime()
    {
        //wait for 0.5 to allow reset/spawn to settle
        yield return new WaitForSeconds(0.5f);

        //set to startForce and enable hovering
        hoverForce = startForce;
        hovering = true;

        //start the lerp to normal hover force which is the set in inspector
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

    public void ResetHoverboard(Vector3 position, Quaternion rotation)
    {
        
        transform.position = position;
        transform.rotation = rotation;

        hovering = false; //stop hovering for stable reset
    
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        StartCoroutine(IncreaseHoverForceOverTime());
    }
}
