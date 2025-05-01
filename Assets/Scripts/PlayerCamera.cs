using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [Header("Offsets")] 
    public Transform player; 
    public Vector3 offset; 
    public Vector3 rotationOffset;

    [Header("Speeds")] 
    public float trackingSpeed;
    public float rotationSpeed;
    public float fovTransitionSpeed ; 

    [Header("FOV")] 
    public float normalFOV;
    public float boostFOV;
   
    private Camera cam;
    private Hoverboard hoverboard;
    private Vector3 velocity; 

    void Start()
    {
        cam = GetComponent<Camera>();
       
        hoverboard = player.GetComponent<Hoverboard>();
        cam.fieldOfView = normalFOV; 
    }

    void FixedUpdate() 
    {
        //position relative to player
        Vector3 targetPosition = player.TransformPoint(offset); 
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, trackingSpeed);

        Quaternion targetRotation = player.rotation * Quaternion.Euler(rotationOffset);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed);

        //adjusting fov based on if boosting
        float targetFOV;
        if (hoverboard.IsBoosting)
        {
            targetFOV = boostFOV; 
        }
        else
        {
            targetFOV = normalFOV; 
        }

        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, Time.fixedDeltaTime * fovTransitionSpeed);
    }
}