using UnityEngine;
using System.Collections;

public class WasteFloorTimer : MonoBehaviour
{
    [Header("Paramètres")]
    [Tooltip("Temps en secondes avant destruction si l'objet reste au sol")]
    public float timeBeforeDestroy = 30f;

    [Tooltip("Points perdus lors de la destruction au sol")]
    public int removeScoreAmount = 1;

    [Header("Effets de Particules")]
    [Tooltip("Effet standard (sur l'objet détruit).")]
    public GameObject standardDestroyEffect;

    // Variable pour stocker le compte à rebours en cours
    private Coroutine destroyCoroutine;

    // Déclenché quand l'objet touche physiquement quelque chose
    private void OnCollisionEnter(Collision collision)
    {
        // On vérifie si c'est bien le sol avec le tag
        if (collision.gameObject.CompareTag("Floor"))
        {
            // S'il n'y a pas déjà un timer en cours, on le lance
            if (destroyCoroutine == null)
            {
                //Debug.Log("Déchet au sol");
                destroyCoroutine = StartCoroutine(TimerSequence());
            }
        }
    }

    // Déclenché quand l'objet quitte le contact du sol, IMPORTANT parce que sinon il se détruit même si on le bouge !
    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Floor"))
        {
            // Si on le ramasse, on annule la destruction !
            if (destroyCoroutine != null)
            {
                StopCoroutine(destroyCoroutine);
                destroyCoroutine = null;
                // Debug.Log("Timer annulé");
            }
        }
    }

    private IEnumerator TimerSequence()
    {
        yield return new WaitForSeconds(timeBeforeDestroy);

        if (GameManager.Instance != null)
        {
            // On utilise AddScore avec un nombre négatif pour retirer des points
            GameManager.Instance.RemoveScore(1);
        }

        // effet standard de particules pour les déchets
        if (standardDestroyEffect != null)
        {
            GameObject fx = Instantiate(standardDestroyEffect, transform.position, Quaternion.identity);

            // Nettoyage automatique
            DestroyParticleAfterPlay(fx);
        }

        Destroy(gameObject);
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