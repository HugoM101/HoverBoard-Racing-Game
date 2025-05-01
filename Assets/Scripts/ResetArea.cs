using UnityEngine;

public class ResetArea : MonoBehaviour
{
    public Transform resetLocation;
    public Hoverboard hoverboard;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            hoverboard.ResetHoverboard(resetLocation.position, resetLocation.rotation);
        }
    }
}