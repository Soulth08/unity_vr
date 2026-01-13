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
    public Color couleurPret = Color.green;   // Vert si disponible
    public Color couleurUrgence = Color.red;  // Rouge si en cours de rechargement

    [Header("Physique & Détection")]
    [Tooltip("Distance physique en mètres pour déclencher l'activation")]
    [SerializeField] private float seuilActivation = 0.12f;

    // Variables internes
    private Vector3 startPosition;
    private bool estDisponible = true;
    private Color couleurPlafondBase; // Pour se souvenir de la couleur originale des luimieres au plafond

    private void Awake()
    {
        startPosition = transform.localPosition;

        // On sauvegarde la couleur normale du plafond pour plus tard, comme ça on pourra la remettre à la fin de la durée de STOP
        if (lumieresPlafond.Length > 0 && lumieresPlafond[0] != null)
        {
            couleurPlafondBase = lumieresPlafond[0].color; // On prend la première lumière au plafond, en partant du principe qu'elles ont toutes la même couleur
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
        // Emergency STOP
        estDisponible = false; // On verrouille le système
        Debug.Log("URGENCE ACTIVÉE : Tapis STOP");

        // Arrêt des treadmills, mais pas du spawner !
        SetTreadmillsPaused(true);

        // Lumières, tout passe au ROUGE
        if (lumierePoignee != null)
        {
            lumierePoignee.enabled = true;
            lumierePoignee.color = couleurUrgence;
        }
        ChangerCouleurPlafond(couleurUrgence);

        // 5 secondes d'arrêt
        yield return new WaitForSeconds(dureeArretUrgence);


        // Cooldown
        Debug.Log("FIN URGENCE : Redémarrage Tapis + Début Cooldown");

        // réactive les treadmills
        SetTreadmillsPaused(false);

        // les lumières au plafond reviennent à leur couleur de base
        ChangerCouleurPlafond(couleurPlafondBase);

        // la lumière d el apoignée cligote en rouge pendant le cooldown
        float tempsRestant = dureeCycleTotal - dureeArretUrgence;
        float finCooldown = Time.time + tempsRestant;

        while (Time.time < finCooldown)
        {
            // On allume/éteint la lumière rouge de la poignée
            if (lumierePoignee != null)
            {
                lumierePoignee.enabled = !lumierePoignee.enabled; // Clignotement, même principe que le texte sur l'ordinateur
                // S'assure qu'elle reste rouge quand elle est allumée
                if (lumierePoignee.enabled) lumierePoignee.color = couleurUrgence;
            }

            yield return new WaitForSeconds(0.25f); // Vitesse du clignotement
        }

        Debug.Log("SYSTÈME PRÊT");

        // Tout redevient vert et disponible
        SetEtatVisuel_Pret();
        estDisponible = true;
    }


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
        foreach (var l in lumieresPlafond) // On parcourt toutes les lumieres et on change leur couleur individuellement
        {
            if (l != null) l.color = c;
        }
    }

    private void SetTreadmillsPaused(bool isPaused)
    {
        foreach (var t in treadmills) // On parcourt tous les tapis et on les arrête ou les redémarre
        {
            if (t != null) t.SetPaused(isPaused);
        }
    }
}