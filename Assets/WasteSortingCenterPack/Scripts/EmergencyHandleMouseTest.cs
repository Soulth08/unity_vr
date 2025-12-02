using UnityEngine;

// Script pour simuler avec la souris dans l’éditeur (clic sur l’objet)
public class EmergencyHandleMouseTest : MonoBehaviour
{
    private EmergencyHandle emergencyHandle;

    void Start()
    {
        emergencyHandle = GetComponent<EmergencyHandle>();
        if (emergencyHandle == null)
        {
            Debug.LogError("Error.");
        }
    }

    void OnMouseDown()
    {
        if (emergencyHandle != null)
            emergencyHandle.OnHandlePulled(null); 
    }

    void OnMouseUp()
    {
        if (emergencyHandle != null)
            emergencyHandle.OnHandleReleased(null); 
    }
}
