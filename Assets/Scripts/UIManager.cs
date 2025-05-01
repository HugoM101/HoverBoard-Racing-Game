using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; } //singleton

    [Header("Text")]
    public TextMeshProUGUI boostText;
    public TextMeshProUGUI positionText;
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI countdownText;
    public TextMeshProUGUI finishText;

    [Header("Sliders")]
    public Slider healthBar;

    [Header("Panels")]
    public GameObject pauseMenuPanel;
    public GameObject deathPanel;
    public GameObject finishScreenPanel;

    //flags
    private bool finishScreenShown = false;
    private bool canBoost = true;
    private bool isBoosting = false;

    private float cooldownRemaining;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        
        else
        {
            Destroy(gameObject);
        }

        //disabling panels on awake so they dont show
        finishScreenPanel.SetActive(false);
        pauseMenuPanel.SetActive(false);
        deathPanel.SetActive(false);
    }

    void Update()
    {
        UpdateBoostDisplay();
    }

    #region Boost
    //updating the boost status and colour depending on the current state
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

    //to be mainly used by the hoverboard script 
    public void UpdateBoostStatus(bool boostAvailable, bool boosting, float cooldown = 0f)
    {
        canBoost = boostAvailable;
        isBoosting = boosting;

        if (!boostAvailable && !boosting) 
        {
            cooldownRemaining = cooldown;
        }
    }
    #endregion

    #region Position
    public void UpdatePositionDisplay(int position, int lapCount)
    {
        positionText.text = $"Position: {position} \n Lap: {lapCount} / 2"; //now 2 afer feedback adaptation
    }
    #endregion
    
    #region Countdown
    public void UpdateCountdownDisplay(float timeRemaining)
    {
        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(true);
            if (timeRemaining > 0)
            {
                countdownText.text = timeRemaining.ToString("F0"); 
            }

            else
            {
                countdownText.text = "Go!";
            }
        }
    }

    public void HideCountdown()
    {
        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(false);
        }
    }
    #endregion

    #region HealthBar
    public void UpdateHealthBar(int currentHealth, int maxHealth = 100)
    {
        if (healthBar != null)
        {
            healthBar.value = (float)currentHealth / maxHealth;
            healthText.text = $"Health: {currentHealth:F0}";
        }
    }
    #endregion

    #region Panels and Screens
    public void ShowFinishScreen(int finalPosition)
    {
        if (finishScreenPanel != null && !finishScreenShown)
        {
            finishScreenShown = true;
            finishScreenPanel.SetActive(true);
            Time.timeScale = 0f; 

            if (finishText != null)
            {
                string suffix;
                switch (finalPosition)
                {
                    case 1:
                        suffix = "st";
                        break;

                    case 2:
                        suffix = "nd";
                        break;

                    case 3:
                        suffix = "rd";
                        break;

                    default:
                        suffix = "th";
                        break;
                }
                
                if (finalPosition == 1)
                {
                    finishText.text = $"Congratulations! You won and finished {finalPosition}{suffix}!";
                }

                else
                {
                    finishText.text = $"Race complete. You placed {finalPosition}{suffix}";
                }
            }
        }
    }

    public void ShowPauseMenu()
    {
        pauseMenuPanel.SetActive(true);
    }

    public void HidePauseMenu()
    {
        pauseMenuPanel.SetActive(false);
    }

    public void ShowDeathScreen()
    {
        deathPanel.SetActive(true);
    }
    #endregion
}