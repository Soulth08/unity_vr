using UnityEngine;

public class EmergencyHandle2 : MonoBehaviour
{
    [Header("Physique")]
    [SerializeField] private float seuilActivation = 0.12f;
    private Vector3 startPosition;
    private bool aEteActive = false;

    [Header("Feedback Visuel")]
    public Light lumierePoignee;

    private void Awake()
    {
        startPosition = transform.localPosition;
    }

    private void Update()
    {
        if (aEteActive) return;

        float distance = Vector3.Distance(transform.localPosition, startPosition);

        if (distance >= seuilActivation)
        {
            if (GameManager2.Instance != null)
            {
                aEteActive = true;
                GameManager2.Instance.StartGame();

                if (lumierePoignee != null)
                    lumierePoignee.color = Color.green;
            }
        }
    }
}