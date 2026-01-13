using UnityEngine;
using System.Collections;

public class DestroyBombBin : MonoBehaviour
{
    [Header("Effets de Particules")]
    [Tooltip("Effet quand on jette autre chose que la bombe")]
    public GameObject errorEffect;

    private void OnTriggerEnter(Collider other)
    {
        // On lance la séquence pour tout objet qui rentre
        StartCoroutine(DestroySequence(other.gameObject));
    }

    private IEnumerator DestroySequence(GameObject objToDestroy)
    {
        yield return new WaitForSeconds(2f);

        if (objToDestroy == null) yield break;

        if (objToDestroy.CompareTag("Bomb"))
        {
            if (GameManager.Instance != null)
            {
                // Comme la poubelle des bombes est éloignée de la zone de tri, on a décidé de grandement récompenser le joueur
                // Gagner des points au score
                GameManager.Instance.AddScore(10);

                // Gagner 1 Vie
                GameManager.Instance.AddLife(1);
            }

            Destroy(objToDestroy);
        }

        else
        {
            if (GameManager.Instance != null)
            {
                // Perdre 1 point
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