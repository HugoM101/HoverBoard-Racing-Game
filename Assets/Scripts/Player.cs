using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public Hoverboard hoverboard;
    float moveInput;
    float turnInput;
    // Start is called before the first frame update
    void Start()
    {
       
    }

    // Update is called once per frame

    //fixed update used as we are applying forces and torques and we need it to be in sync with physics engine
    void FixedUpdate()
    {
        moveInput = Input.GetAxis("Vertical");
        turnInput = Input.GetAxis("Horizontal");
        
        hoverboard.Move(moveInput, turnInput);
    }
}
