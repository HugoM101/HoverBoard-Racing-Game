using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public Text boostText;
    private float cooldownRemaining;
    private bool canBoost = true;
    private bool isBoosting = false;

    public Text positionText;

    void Update()
    {
        UpdateBoostDisplay();
    }

    void UpdateBoostDisplay()
    {
        if (isBoosting)
        {
            boostText.color = new Color(1f, 0.5f, 0f); //amber
            boostText.text = "Boosting";
        }

        else if (!canBoost && cooldownRemaining > 0)
        {
            cooldownRemaining -= Time.deltaTime;
            boostText.color = Color.red;
            boostText.text = "Boost Cooldown: " + Mathf.Ceil(cooldownRemaining).ToString("F0");
        }

        else if (canBoost)
        {
            boostText.color = Color.green;
            boostText.text = "Boost Available";
        }
    }

    public void UpdateBoostStatus(bool boostAvailable, bool boosting, float cooldown = 0f)
    {
        canBoost = boostAvailable;
        isBoosting = boosting;

        if (!boostAvailable && !boosting) 
        {
            cooldownRemaining = cooldown;
        }
    }

    public void UpdatePositionDisplay(int position, int lapCount)
    {
        positionText.text = $"Position: {position} \n Lap: {lapCount}";
    }
}