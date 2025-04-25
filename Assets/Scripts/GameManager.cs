using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    private float countdownTime = 3f;
    private bool gameStarted = false;
    public bool IsGameStarted { get { return gameStarted; } }

    private bool raceFinished = false;
    private int lapsToFinish = 3; //3 as it needs to complete the 2nd lap in total

    private bool isPaused = false;



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
    }

    void Update()
    {
        if (!raceFinished && gameStarted && !isPaused)
        {
            CheckRaceCompletion();
        }
        
        //if p pressed and game has begun + race not over
        if (Input.GetKeyDown(KeyCode.P) && gameStarted && !raceFinished) 
        {
            if (!isPaused)
            {
                PauseGame();
            }
        }
    }

    void Start()
    {
        if (UIManager.Instance == null)
        {
            return;
        }

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        StartCoroutine(Countdown());
    }

    IEnumerator Countdown()
    {
        while (countdownTime > 0)
        {
            UIManager.Instance.UpdateCountdownDisplay(Mathf.Ceil(countdownTime));
            yield return null;
            countdownTime -= Time.deltaTime;
        }

        //countdown now 0 so allow game to be able to start
        UIManager.Instance.UpdateCountdownDisplay(0);

        gameStarted = true;

        //allows for the "Go!" text to show for a second before hiding the countdown UI
        yield return new WaitForSeconds(1f);
        UIManager.Instance.HideCountdown();
    }

    private void CheckRaceCompletion()
    {
        if (PositionTracker.Instance != null)
        {
            foreach (var racer in PositionTracker.Instance.racers)
            {
                if (racer.isPlayer && racer.lapCount >= lapsToFinish)
                {
                    raceFinished = true;
                    Cursor.visible = true;
                    Cursor.lockState = CursorLockMode.None;
                    UIManager.Instance.ShowFinishScreen(racer.currentPosition);
                    break;
                }
            }
        }
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            Time.timeScale = 1f;
        }
    }

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;
        UIManager.Instance.ShowPauseMenu();
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        UIManager.Instance.HidePauseMenu();
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    
    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void OnPlayerDeath()
    {
        Time.timeScale = 0f;
        UIManager.Instance.ShowDeathScreen();
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
}