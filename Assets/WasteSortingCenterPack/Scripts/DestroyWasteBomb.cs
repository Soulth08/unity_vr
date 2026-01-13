using UnityEngine;
using System.Collections;

public class DestroyBombBin : MonoBehaviour
{
    [Header("Effets de Particules")]
    [Tooltip("Effet positif quand une bombe est neutralisée (ex: Étincelles vertes / Confettis)")]
    public GameObject neutralizeEffect;

    [Tooltip("Effet d'erreur quand on jette autre chose (ex: Fumée noire / Croix rouge)")]
    public GameObject errorEffect;

    private void OnTriggerEnter(Collider other)
    {
        // On lance la séquence pour tout objet qui rentre
        StartCoroutine(DestroySequence(other.gameObject));
    }

    private IEnumerator DestroySequence(GameObject objToDestroy)
    {
        // 1. Délai pour laisser l'objet tomber au fond
        yield return new WaitForSeconds(2f);

        if (objToDestroy == null) yield break;

        // --- CAS 1 : C'EST UNE BOMBE (VICTOIRE) ---
        if (objToDestroy.CompareTag("Bomb"))
        {
            if (GameManager.Instance != null)
            {
                // A. Gagner 10 Points (On passe 10 en argument)
                GameManager.Instance.AddScore(10);

                // B. Gagner 1 Vie (Nécessite la fonction AddLife ajoutée précédemment)
                GameManager.Instance.AddLife(1);
            }

            // C. Effet visuel de réussite (Neutralisation)
            if (neutralizeEffect != null)
            {
                GameObject fx = Instantiate(neutralizeEffect, objToDestroy.transform.position, Quaternion.identity);
                DestroyParticleAfterPlay(fx);
            }

            // D. Destruction (Neutralise la bombe car son script Countdown est détruit avec)
            Destroy(objToDestroy);
        }

        // --- CAS 2 : C'EST UN DÉCHET OU AUTRE (ERREUR) ---
        else
        {
            if (GameManager.Instance != null)
            {
                // Perdre 1 point (Score négatif)
                GameManager.Instance.AddScore(-1);
            }

            // Effet visuel d'erreur
            if (errorEffect != null)
            {
                GameObject fx = Instantiate(errorEffect, objToDestroy.transform.position, Quaternion.identity);
                DestroyParticleAfterPlay(fx);
            }

            Destroy(objToDestroy);
        }
    }

    private void DestroyParticleAfterPlay(GameObject particleObject)
    {
        ParticleSystem ps = particleObject.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            float totalDuration = ps.main.duration + ps.main.startLifetime.constantMax;
            Destroy(particleObject, totalDuration);
        }
        else
        {
            Destroy(particleObject, 3f);
        }
    }
}