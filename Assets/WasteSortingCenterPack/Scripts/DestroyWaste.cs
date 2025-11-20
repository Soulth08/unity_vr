using UnityEngine;

public class DestroyWaste : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Waste"))
        {
            Destroy(other.gameObject);
        }
    }
}