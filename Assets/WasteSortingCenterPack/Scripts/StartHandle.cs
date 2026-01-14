using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRGrabInteractable))]
public class StartHandle : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private float seuilActivation = 0.12f;
    [SerializeField] private float delaiEntreDeuxTirages = 1.0f; // Temps avant de pouvoir tirer à nouveau la poignée

    [Header("Lumière de la Poignée")]
    public Light lumierePoignee;
    public Color couleurRepos = Color.red;
    public Color couleurActive = Color.green;

    // Variables internes
    private Vector3 startPosition;
    private bool estVerrouillee = false;

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
        SetLumiere(couleurActive); // on change la couleur de la lumière de la poignée

        Debug.Log("POIGNÉE TIRÉE !");

        //on fait clignoter la lumière de la poignée
        for (int i = 0; i < 3; i++)
        {
            if (lumierePoignee != null)
            {
                lumierePoignee.enabled = false;
                yield return new WaitForSeconds(0.3f);
                lumierePoignee.enabled = true;
                yield return new WaitForSeconds(0.3f);
            }
        }

        // On prévient le GameManager qu'une action a eu lieu
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartHandle();
        }

        // on attend un peu avant de permettre de tirer à nouveau (Cooldown)
        yield return new WaitForSeconds(delaiEntreDeuxTirages);

        // On remet la lumière de base et on déverrouille
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
}