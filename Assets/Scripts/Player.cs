using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public Hoverboard hoverboard;
    float moveInput;
    float turnInput;
    bool boostTriggered = false;
    private int health = 100;
    private int maxHealth = 100;

    public int Health { get { return health; } }

    public float TurnInput { get { return turnInput; } }
   
    void Start()
    {
       PositionTracker.Instance.RegisterRacer(transform, true, 0f);

       UIManager.Instance.UpdateHealthBar(health, maxHealth);
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
        if (GameManager.Instance != null && GameManager.Instance.IsGameStarted)
        {
            hoverboard.Move(moveInput, turnInput);
            if (boostTriggered)
            {
                hoverboard.Boost();
                boostTriggered = false;
            }
        }
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        if (health < 0) health = 0;
        Debug.Log(health);

        // Update health bar
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateHealthBar(health, maxHealth);
        }

        if (health <= 0)
        {
            Debug.Log("died");
        }
    }
}