using UnityEngine;

public class VisualHoverboard : MonoBehaviour
{
    [Header("Tilt Settings")] 
    public float tiltAngle;
    public float tiltSpeed;
    private float currentTilt;

    [Header("Tilt Axis Config")] 
    public TiltAxis tiltAxis; 
    
    //ability to choose which axis to apply tilt on
    public enum TiltAxis
    {
        x,  
        y,  
        z  
    }

    [Header("References")] 
    public Transform targetHoverboard;
    public Player player;

    void Update()
    {   
        //matching the position with the physics hoverboard
        transform.position = targetHoverboard.position;

        float turnInput = player.TurnInput;

        //calculating the target tilt from the input and clamping
        float targetTilt = Mathf.Clamp(-turnInput * tiltAngle, -tiltAngle, tiltAngle);

        //smoothly interpolating to avoid suddent rigid tilting
        currentTilt = Mathf.Lerp(currentTilt, targetTilt, Time.deltaTime * tiltSpeed);

        Vector3 tiltVector = Vector3.zero;

        //converting to rot vector based on my chosen axis
        switch (tiltAxis)
        {
            case TiltAxis.x:
                tiltVector = new Vector3(currentTilt, 0f, 0f); 
                break;
            case TiltAxis.y:
                tiltVector = new Vector3(0f, currentTilt, 0f); 
                break;
            case TiltAxis.z:
                tiltVector = new Vector3(0f, 0f, currentTilt); 
                break;
        }

        //apply new rotation to this transform without affecting physics hoverboard transfrom.
        Quaternion tiltRotation = Quaternion.Euler(tiltVector);
        transform.rotation = targetHoverboard.rotation * tiltRotation;
    }
}