using UnityEngine;
using System.Collections;
using System;

public class DestroyWasteBin : MonoBehaviour
{
    [Header("Effets de Particules")]
    [Tooltip("Effet standard (sur l'objet détruit).")]
    public GameObject standardDestroyEffect;

    [Tooltip("Effet d'explosion de la bombe")]
    public GameObject bombDestroyEffect;
    public Quaternion DeleterParticlesDirection;

    private void OnTriggerEnter(Collider other)
    {
        StartCoroutine(DestroySequence(other.gameObject));
    }

    private IEnumerator DestroySequence(GameObject objToDestroy)
    {
        yield return new WaitForSeconds(2f);

        if (objToDestroy == null) yield break;

        // effet standard de particules pour les déchets
        if (standardDestroyEffect != null)
        {
            GameObject fx = Instantiate(standardDestroyEffect, objToDestroy.transform.position, Quaternion.identity);

            // Nettoyage automatique
            DestroyParticleAfterPlay(fx);
        }

        // si un objet recyclable arrive dans le deleter, on ajoute un point au score
        if (objToDestroy.CompareTag("Waste Recycle"))
        {

            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddScore(); // on utilise les fonctions du gameManager

            }

            Destroy(objToDestroy);
        }

        // Bombe
        else if (objToDestroy.CompareTag("Bomb"))
        {
            if (bombDestroyEffect != null)
            {
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.TakeDamage(1, true); // si une bombe arrive dans une poubelle, on perd directement !
                }

                GameObject errorFx = Instantiate(bombDestroyEffect, transform.position, transform.rotation * DeleterParticlesDirection);

                // Nettoyage automatique
                DestroyParticleAfterPlay(errorFx);
            }

            Destroy(objToDestroy);
        }


        else
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddScore(-1); // si un objet non recyclable arrive dans la poubelle, on perd un point au score
            }
            Destroy(objToDestroy);
        }

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