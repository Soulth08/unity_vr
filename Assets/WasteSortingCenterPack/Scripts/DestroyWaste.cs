using UnityEngine;

public class DestroyWaste : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Waste"))
        {
            //attendre 2 secondes avant de detruire l'objet
            Destroy(other.gameObject, 2f);
        }
    }
}