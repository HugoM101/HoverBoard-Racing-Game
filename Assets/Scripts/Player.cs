using UnityEngine;

public class Player : MonoBehaviour
{
    public Hoverboard hoverboard;

    [Header("Inputs")]
    private float moveInput;
    private float turnInput;
    public float TurnInput { get { return turnInput; } } //needed by the visual hoverboard script

    [Header("Health")]
    private int health;
    private int maxHealth;
    private bool boostTriggered;

    void Start()
    {
        maxHealth = 100;
        health = 100;
        PositionTracker.Instance.RegisterRacer(transform, true, 0.97f);
        UIManager.Instance.UpdateHealthBar(health, maxHealth);
        boostTriggered = false;
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

        CheckDeath();
    }

    //fixed update used as we are applying forces and torques and we need it to be in sync with physics engine
    //causing stutters and inconsistencies in normal update
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
        if (health < 0)
        {
            health = 0;
        }

        UIManager.Instance.UpdateHealthBar(health, maxHealth);
    }

    public void CheckDeath()
    {
        if (health == 0)
        {
            GameManager.Instance.OnPlayerDeath();
        }
    }
}