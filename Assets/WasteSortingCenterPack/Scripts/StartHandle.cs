using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRGrabInteractable))]
public class StartHandle : MonoBehaviour
{
    [Header("Configuration")]
    [Tooltip("Distance physique en mètres pour déclencher l'activation")]
    [SerializeField] private float seuilActivation = 0.12f;

    [Header("Lumière de la Poignée")]
    [Tooltip("La lumière située sur la poignée")]
    public Light lumierePoignee;

    [Header("Couleurs")]
    public Color couleurInactif = Color.red;   // Rouge = pas encore tiré
    public Color couleurActif = Color.green;   // Vert = partie lancée

    // Variables internes
    private Vector3 startPosition;
    private bool aEteActive = false; // Pour s'assurer qu'on ne déclenche qu'une seule fois

    private void Awake()
    {
        startPosition = transform.localPosition;

        // Au début, la poignée est rouge (inactive)
        SetLumiereInactive();
    }

    private void Update()
    {
        // Si déjà activée, on ne fait plus rien
        if (aEteActive) return;

        // On calcule de combien la poignée a bougé
        float currentDistance = Vector3.Distance(transform.localPosition, startPosition);

        // Si on dépasse le seuil, on lance la partie
        if (currentDistance >= seuilActivation)
        {
            ActiverPoignee();
        }
    }

    private void ActiverPoignee()
    {
        aEteActive = true;
        Debug.Log("START HANDLE : Partie lancée !");

        // La lumière passe au vert
        SetLumiereActive();

        // --- APPEL DES ACTIONS DE DÉMARRAGE ---

        // 1. Notifier le GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartGame();
        }
        else
        {
            Debug.LogWarning("GameManager.Instance est null !");
        }

        // --- TU PEUX AJOUTER D'AUTRES ACTIONS ICI ---

        // Exemple : Démarrer les tapis
        // TreadmillsController[] tapis = FindObjectsOfType<TreadmillsController>();
        // foreach (var t in tapis)
        // {
        //     if (t != null) t.SetPaused(false);
        // }

        // Exemple : Activer un spawner de déchets
        // WasteSpawner spawner = FindObjectOfType<WasteSpawner>();
        // if (spawner != null) spawner.StartSpawning();

        // Exemple : Démarrer un timer
        // TimerController timer = FindObjectOfType<TimerController>();
        // if (timer != null) timer.StartTimer();
    }

    private void SetLumiereInactive()
    {
        if (lumierePoignee != null)
        {
            lumierePoignee.enabled = true;
            lumierePoignee.color = couleurInactif;
        }
    }

    private void SetLumiereActive()
    {
        if (lumierePoignee != null)
        {
            lumierePoignee.enabled = true;
            lumierePoignee.color = couleurActif;
        }
    }

    // Dessine une sphère jaune dans l'éditeur pour voir la distance d'activation
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 center = Application.isPlaying ? startPosition : transform.localPosition;
        Gizmos.DrawWireSphere(center, seuilActivation);
    }
}