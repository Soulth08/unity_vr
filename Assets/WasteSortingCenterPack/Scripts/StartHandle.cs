using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
// Décommente si tu utilises XR Toolkit 3.0+
// using UnityEngine.XR.Interaction.Toolkit.Interactables; 

[RequireComponent(typeof(XRGrabInteractable))]
public class StartHandle : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private float seuilActivation = 0.12f;
    [SerializeField] private float delaiEntreDeuxTirages = 1.0f; // Temps avant de pouvoir retirer

    [Header("Lumière de la Poignée")]
    public Light lumierePoignee;
    public Color couleurRepos = Color.red;
    public Color couleurActive = Color.green;

    // Variables internes
    private Vector3 startPosition;
    private bool estVerrouillee = false; // Remplace "aEteActive" pour gérer le cooldown

    private void Awake()
    {
        startPosition = transform.localPosition;
        SetLumiere(couleurRepos);
    }

    private void Update()
    {
        // Si la poignée est en cooldown (verrouillée), on ne fait rien
        if (estVerrouillee) return;

        float currentDistance = Vector3.Distance(transform.localPosition, startPosition);

        if (currentDistance >= seuilActivation)
        {
            StartCoroutine(ActionSequence());
        }
    }

    private IEnumerator ActionSequence()
    {
        estVerrouillee = true; // On verrouille immédiatement
        SetLumiere(couleurActive); // Feedback visuel

        Debug.Log("POIGNÉE TIRÉE !");

        // On prévient le GameManager qu'une action a eu lieu
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnHandleAction();
        }

        // On attend un peu avant de permettre de tirer à nouveau (Cooldown)
        yield return new WaitForSeconds(delaiEntreDeuxTirages);

        // On remet la lumière au rouge (repos) et on déverrouille
        SetLumiere(couleurRepos);
        estVerrouillee = false;
    }

    private void SetLumiere(Color c)
    {
        if (lumierePoignee != null)
        {
            lumierePoignee.enabled = true;
            lumierePoignee.color = c;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 center = Application.isPlaying ? startPosition : transform.localPosition;
        Gizmos.DrawWireSphere(center, seuilActivation);
    }
}