using UnityEngine;
using System.Collections;

public class DestroyWaste : MonoBehaviour
{
    [Header("Effets de Particules")]
    [Tooltip("Effet standard (sur l'objet détruit).")]
    public GameObject standardDestroyEffect;

    [Tooltip("Effet négatif (sur le Deleter).")]
    public GameObject negativeDestroyEffect;

    public Quaternion DeleterParticlesDirection;

    private void OnTriggerEnter(Collider other)
    {
        StartCoroutine(DestroySequence(other.gameObject));
    }

    private IEnumerator DestroySequence(GameObject objToDestroy)
    {
        yield return new WaitForSeconds(2f);

        if (objToDestroy == null) yield break;

        // --- 1. EFFET STANDARD (SUR L'OBJET) ---
        if (standardDestroyEffect != null)
        {
            // On garde Quaternion.identity ici pour que l'explosion soit droite par rapport au monde
            // (Mais on peut changer si tu veux qu'elle suive l'objet)
            GameObject fx = Instantiate(standardDestroyEffect, objToDestroy.transform.position, Quaternion.identity);

            // Nettoyage automatique
            DestroyParticleAfterPlay(fx);
        }

        // --- 2. EFFET NEGATIF (SUR LE DELETER) ---
        if (objToDestroy.CompareTag("Waste Recycle"))
        {
            if (negativeDestroyEffect != null)
            {
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.TakeDamage(1);
                }

                // CORRECTION DIRECTION : On utilise 'transform.rotation' au lieu de 'Quaternion.identity'
                // L'effet suivra désormais la rotation de votre objet Deleter
                GameObject errorFx = Instantiate(negativeDestroyEffect, transform.position, transform.rotation * DeleterParticlesDirection);

                // Nettoyage automatique
                DestroyParticleAfterPlay(errorFx);
            }
        }

        Destroy(objToDestroy);
    }

    // Petite fonction utilitaire pour nettoyer les particules
    private void DestroyParticleAfterPlay(GameObject particleObject)
    {
        ParticleSystem ps = particleObject.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            // On calcule la durée totale (durée de l'émission + durée de vie max des particules)
            float totalDuration = ps.main.duration + ps.main.startLifetime.constantMax;

            // On détruit l'objet après ce temps
            Destroy(particleObject, totalDuration);
        }
        else
        {
            // Sécurité : si pas de ParticleSystem, on détruit après 3 secondes par défaut
            Destroy(particleObject, 3f);
        }
    }
}