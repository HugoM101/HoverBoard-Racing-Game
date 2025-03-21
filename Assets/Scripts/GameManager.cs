using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    private float countdownTime = 3f;
    private bool gameStarted = false;
    public bool IsGameStarted 
    {
        get { return gameStarted; }
    }



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

    void Start()
    {
        if (UIManager.Instance == null)
        {
            return;
        }

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

        //allow for "Race!" text to show for a second before hiding the countdown UI
        yield return new WaitForSeconds(1f);
        UIManager.Instance.HideCountdown();
    }
}