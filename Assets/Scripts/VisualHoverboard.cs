using UnityEngine;

public class VisualHoverboard : MonoBehaviour
{
    public Transform targetHoverboard;
    public Player player;
    public float tiltAngle = 30f;
    public float tiltSpeed = 5f;
    
    //allow for changing tilt direction 
    public enum TiltAxis
    {
        x,  
        y,  
        z  
    }
    public TiltAxis tiltAxis;
    private Quaternion initialRotation;
    private float currentTilt = 0f;

    void Start()
    {
        initialRotation = transform.localRotation;
    }

    void Update()
    {
        transform.position = targetHoverboard.position;
        float turnInput = player.TurnInput;

        float targetTilt = Mathf.Clamp(-turnInput * tiltAngle, -tiltAngle, tiltAngle);
        currentTilt = Mathf.Lerp(currentTilt, targetTilt, Time.deltaTime * tiltSpeed);

        Vector3 tiltVector = Vector3.zero;

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

        //apply rot in local space
        Quaternion tiltRotation = Quaternion.Euler(tiltVector);
        transform.rotation = targetHoverboard.rotation * tiltRotation;
    }
}