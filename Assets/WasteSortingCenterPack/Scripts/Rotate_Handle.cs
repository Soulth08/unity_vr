using UnityEngine;

public class Rotate_Handle : MonoBehaviour
{
    [Header("Liaison")]
    [SerializeField] private TreadmillsController treadmillsController;

    [Header("Configuration Angle")]
    // Axe de rotation (X pour le rouge)
    [SerializeField] private Vector3 rotationAxis = Vector3.right;

    // Définis ici l'amplitude de mouvement.
    // Exemple : -45 (arrière) à 45 (avant).
    // Si la poignée est à 0 (verticale), la vitesse sera pile au milieu (0.5).
    [SerializeField] private float angleBack = -45f;
    [SerializeField] private float angleForward = 45f;

    void Update()
    {
        if (treadmillsController == null) return;

        // 1. Récupérer l'angle (-180 à 180)
        float currentAngle = GetSignedAngle();

        // 2. Transformer l'angle en ratio (0 à 1)
        // Si angle = -45 -> ratio 0 (vitesse min)
        // Si angle = 0   -> ratio 0.5 (vitesse moyenne)
        // Si angle = 45  -> ratio 1 (vitesse max)
        float ratio = Mathf.InverseLerp(angleBack - 10f, angleForward, currentAngle);

        // 3. Envoyer au contrôleur
        treadmillsController.SetTargetSpeed(ratio);
    }

    private float GetSignedAngle()
    {
        float angle = 0f;

        if (rotationAxis == Vector3.right) angle = transform.localEulerAngles.x;
        else if (rotationAxis == Vector3.up) angle = transform.localEulerAngles.y;
        else if (rotationAxis == Vector3.forward) angle = transform.localEulerAngles.z;

        // Conversion pour avoir des angles comme -10, -20 au lieu de 350, 340
        if (angle > 180) angle -= 360;

        return angle;
    }
}