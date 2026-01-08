using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections;
using UnityEngine.XR.Interaction.Toolkit.Interactables; // Nécessaire pour XR Toolkit récent

[RequireComponent(typeof(XRGrabInteractable))]
public class EmergencyHandleTimed : MonoBehaviour
{
    [Header("Paramètres de Temps")]
    [Tooltip("Temps total avant de pouvoir réutiliser la poignée (20s)")]
    [SerializeField] private float dureeCycleTotal = 20f;

    [Tooltip("Durée de l'arrêt effectif des tapis (5s)")]
    [SerializeField] private float dureeArretUrgence = 5f;

    [Header("Liaisons Tapis")]
    [SerializeField] private TreadmillsController[] treadmills;

    [Header("Configuration Lumières")]
    [Tooltip("La lumière située sur la poignée elle-même")]
    public Light lumierePoignee;

    [Tooltip("Les lumières au plafond de la salle")]
    public Light[] lumieresPlafond;

    [Header("Couleurs")]
    public Color couleurPret = Color.green;   // Vert
    public Color couleurUrgence = Color.red;  // Rouge

    [Header("Physique & Détection")]
    [Tooltip("Distance physique en mètres pour déclencher l'activation")]
    [SerializeField] private float seuilActivation = 0.12f;

    // Variables internes
    private Vector3 startPosition;
    private bool estDisponible = true; // Est-ce qu'on peut tirer ?
    private Color couleurPlafondBase; // Pour se souvenir de la couleur originale du plafond

    private void Awake()
    {
        startPosition = transform.localPosition;

        // On sauvegarde la couleur normale du plafond pour plus tard
        if (lumieresPlafond.Length > 0 && lumieresPlafond[0] != null)
        {
            couleurPlafondBase = lumieresPlafond[0].color;
        }

        // On met tout au vert au démarrage
        SetEtatVisuel_Pret();
    }

    private void Update()
    {
        // Si la poignée est en cooldown ou déjà tirée, on ignore la physique
        if (!estDisponible) return;

        // On calcule de combien la poignée a bougé
        float currentDistance = Vector3.Distance(transform.localPosition, startPosition);

        // Si on dépasse le seuil, on lance la séquence
        if (currentDistance >= seuilActivation)
        {
            StartCoroutine(SequenceUrgence());
        }
    }

    private IEnumerator SequenceUrgence()
    {
        // --- PHASE 1 : ARRÊT D'URGENCE (0s à 5s) ---
        estDisponible = false; // On verrouille le système
        Debug.Log("URGENCE ACTIVÉE : Tapis STOP");

        // 1. Arrêt des tapis
        SetTreadmillsPaused(true);

        // 2. Lumières : Tout passe au ROUGE FIXE
        if (lumierePoignee != null)
        {
            lumierePoignee.enabled = true;
            lumierePoignee.color = couleurUrgence;
        }
        ChangerCouleurPlafond(couleurUrgence);

        // 3. On attend les 5 secondes d'arrêt
        yield return new WaitForSeconds(dureeArretUrgence);


        // --- PHASE 2 : REDÉMARRAGE & COOLDOWN (5s à 20s) ---
        Debug.Log("FIN URGENCE : Redémarrage Tapis + Début Cooldown");

        // 1. Les tapis redémarrent
        SetTreadmillsPaused(false);

        // 2. Le Plafond redevient NORMAL (couleur de base)
        ChangerCouleurPlafond(couleurPlafondBase);

        // 3. La Poignée CLIGNOTE ROUGE pendant le temps restant
        // Calcul du temps restant : 20 - 5 = 15 secondes
        float tempsRestant = dureeCycleTotal - dureeArretUrgence;
        float finCooldown = Time.time + tempsRestant;

        while (Time.time < finCooldown)
        {
            // On allume/éteint la lumière rouge de la poignée
            if (lumierePoignee != null)
            {
                lumierePoignee.enabled = !lumierePoignee.enabled;
                // S'assure qu'elle reste rouge quand elle est allumée
                if (lumierePoignee.enabled) lumierePoignee.color = couleurUrgence;
            }

            yield return new WaitForSeconds(0.25f); // Vitesse du clignotement
        }

        // --- PHASE 3 : RETOUR À LA NORMALE (Après 20s) ---
        Debug.Log("SYSTÈME PRÊT");

        // Tout redevient vert et disponible
        SetEtatVisuel_Pret();
        estDisponible = true;
    }

    // --- Fonctions d'aide ---

    private void SetEtatVisuel_Pret()
    {
        // Poignée : Verte et allumée fixe
        if (lumierePoignee != null)
        {
            lumierePoignee.enabled = true;
            lumierePoignee.color = couleurPret;
        }

        // Plafond : Couleur normale
        ChangerCouleurPlafond(couleurPlafondBase);
    }

    private void ChangerCouleurPlafond(Color c)
    {
        foreach (var l in lumieresPlafond)
        {
            if (l != null) l.color = c;
        }
    }

    private void SetTreadmillsPaused(bool isPaused)
    {
        foreach (var t in treadmills)
        {
            if (t != null) t.SetPaused(isPaused);
        }
    }

    // Dessine une sphère jaune dans l'éditeur pour voir la distance d'activation
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 center = Application.isPlaying ? startPosition : transform.position;
        Gizmos.DrawWireSphere(center, seuilActivation);
    }
}