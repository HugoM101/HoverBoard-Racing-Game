using UnityEngine;
using UnityEngine.UI;

public class Settings : MonoBehaviour
{
    [Header("Sliders")]
    public Slider turningSpeedSlider;
    public Slider angularDragSlider;

    [Header("Hoverboard script")]
    public Hoverboard hoverboard;

    void Start()
    {
        turningSpeedSlider.value = hoverboard.turningSpeed;
        turningSpeedSlider.onValueChanged.AddListener(OnTurnSpeedChanged);
        
        angularDragSlider.value = hoverboard.GetComponent<Rigidbody>().angularDrag;
        angularDragSlider.onValueChanged.AddListener(OnAngularDragChanged);
    }

    //update with new values
    void OnTurnSpeedChanged(float newValue)
    {
        hoverboard.turningSpeed = newValue;
    }

    void OnAngularDragChanged(float newValue)
    {
        hoverboard.GetComponent<Rigidbody>().angularDrag = newValue;
    }
}