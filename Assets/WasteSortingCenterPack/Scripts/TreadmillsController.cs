using UnityEngine;

public class TreadmillsController : MonoBehaviour
{
    [SerializeField] float maxTreadmillSpeed = 0.5f;
    [SerializeField, Range(0, 1)] float targetSpeed = 0.5f;
    [SerializeField] Material treadmillMat;
    TreadmillForce[] treadmills;
    const float MATERIAL_SPEED_MULTIPLIER = 1f;

    [Header("Pause")]
    public bool isPaused { get; private set; }

    float currentSpeed, refSpeed;
    const float SPEED_SMOOTH = 0.2f;

    // Nouveau : accumulateur d'offset UV
    float uvOffset = 0f;

    void Start()
    {
        treadmills = FindObjectsByType<TreadmillForce>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
    }

    private void Update()
    {
        float effectiveTargetSpeed = isPaused ? 0 : targetSpeed;
        currentSpeed = Mathf.SmoothDamp(currentSpeed, effectiveTargetSpeed, ref refSpeed, SPEED_SMOOTH);

        // Accumuler l'offset basé sur la vitesse actuelle
        uvOffset += currentSpeed * maxTreadmillSpeed * MATERIAL_SPEED_MULTIPLIER * Time.deltaTime;

        SetSpeed(currentSpeed);
    }

    public void SetSpeed(float speed01)
    {
        float speed = speed01 * maxTreadmillSpeed;

        foreach (TreadmillForce t in treadmills)
        {
            t.SetSpeed(speed);
        }

        // Passer l'offset accumulé au lieu de la vitesse
        treadmillMat.SetFloat("_UVOffset", uvOffset);
    }

    public void SetPaused(bool value)
    {
        isPaused = value;
    }

    public void SetTargetSpeed(float speed01)
    {
        targetSpeed = Mathf.Clamp01(speed01);
    }
}