using UnityEngine;

public class Rotate_Handle : MonoBehaviour
{
    [Header("Liaison")]
    [SerializeField] private TreadmillsController treadmillsController;

    [Header("Configuration Angle")]
    // Axe de rotation (X pour le rouge)
    [SerializeField] private Vector3 rotationAxis = Vector3.right;
    // amplitude de mouvement
    [SerializeField] private float angleBack = -45f;
    [SerializeField] private float angleForward = 45f;

    [Header("Configuration Vitesse")]
    [SerializeField, Range(0f, 1f)] private float minSpeedRatio = 0.2f;
    [SerializeField, Range(0f, 1f)] private float maxSpeedRatio = 1f;

    void Update()
    {
        if (treadmillsController == null) return;

        // récupérer l'angle (-180 à 180)
        float currentAngle = GetSignedAngle();

        // on transforme l'angle en ratio normalisé (0 à 1)
        // Si angle = angleBack ->0
        // Si angle = 0         -> 0.5
        // Si angle = angleForward -> 1
        float normalizedRatio = Mathf.InverseLerp(angleBack, angleForward, currentAngle);

        // ratio doit respecter min/max, éviter d'avoir une vitesse des treamills = 0, ce qui serait identique au emergency STOP
        // normalizedRatio 0 -> minSpeedRatio
        // normalizedRatio 1 -> maxSpeedRatio
        float finalRatio = Mathf.Lerp(minSpeedRatio, maxSpeedRatio, normalizedRatio);

        treadmillsController.SetTargetSpeed(finalRatio);
    }

    private float GetSignedAngle()
    {
        float angle = 0f;

        if (rotationAxis == Vector3.right)
            angle = transform.localEulerAngles.x;
        else if (rotationAxis == Vector3.up)
            angle = transform.localEulerAngles.y;
        else if (rotationAxis == Vector3.forward)
            angle = transform.localEulerAngles.z;

        // Conversion pour avoir des angles comme -10, -20 au lieu de 350, 340
        if (angle > 180)
            angle -= 360;

        return angle;
    }
}