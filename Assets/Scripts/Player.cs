using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public Hoverboard hoverboard;
    float moveInput;
    float turnInput;
    bool boostTriggered = false;
    private int health;
    // Start is called before the first frame update
    void Start()
    {
       PositionTracker.Instance.RegisterRacer(transform, true, 0f);
    }

    void Update()
    {
        //need input to register every frame
        moveInput = Input.GetAxis("Vertical");
        turnInput = Input.GetAxis("Horizontal");

        if (Input.GetKeyDown(KeyCode.Space))
        {
            boostTriggered = true;
        }
    }

    //fixed update used as we are applying forces and torques and we need it to be in sync with physics engine
    void FixedUpdate()
    {
        hoverboard.Move(moveInput, turnInput);
        if (boostTriggered)
        {
            hoverboard.Boost();
            boostTriggered = false;
        }
    }
}
