using UnityEngine;

public class DamageTrigger : MonoBehaviour
{
    public Player player; 
    private float damageCooldown; //time between damage
    private float cooldownTimer;

    void Start()
    {
        cooldownTimer = 0f;
        damageCooldown = 1.0f;
    }

    void Update()
    {
        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer < 0f)
            {
                cooldownTimer = 0f; //make sure it doesnt go negative
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Spike"))
        {
            if (cooldownTimer <= 0f) 
            {
                player.TakeDamage(20);
                cooldownTimer = damageCooldown; //reset
            }
        }
    }
}