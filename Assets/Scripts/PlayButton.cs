using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayButton : MonoBehaviour
{
    void Start()
    {
        Button button = GetComponent<Button>();
       
        button.onClick.AddListener(StartGame);
    }

    void StartGame()
    {
        SceneManager.LoadScene("RaceTrack1"); 
    }
}