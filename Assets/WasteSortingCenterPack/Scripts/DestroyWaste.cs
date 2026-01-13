using UnityEngine;
using System.Collections;

public class DestroyWaste : MonoBehaviour // ce code est utilisé par le deleter principal après les treadmills
{
    [Header("Effets de Particules")]
    [Tooltip("Effet standard (sur l'objet détruit).")]
    public GameObject standardDestroyEffect;

    [Tooltip("Effet négatif (sur le Deleter).")]
    public GameObject negativeDestroyEffect;

    [Tooltip("Effet d'explosion de la bombe")]
    public GameObject bombDestroyEffect;

    public Quaternion DeleterParticlesDirection; // permet de changer la direction des particules pour correspondre à celle du deleter

    private void OnTriggerEnter(Collider other)
    {
        StartCoroutine(DestroySequence(other.gameObject));
    }

    private IEnumerator DestroySequence(GameObject objToDestroy)
    {
        yield return new WaitForSeconds(2f); // attendre un peu avant, sinon l'objet disparait dès qu'il rentre en contact avec le deleter et c'est moche

        if (objToDestroy == null || objToDestroy.CompareTag("Player")) yield break; //éviter que le joueur puisse être lui aussi détruit


        // effet standard de particules, pour les déchets
        if (standardDestroyEffect != null)
        {
            GameObject fx = Instantiate(standardDestroyEffect, objToDestroy.transform.position, Quaternion.identity);

            // Nettoyage automatique
            DestroyParticleAfterPlay(fx);
        }

        // effet lorsqu'un déchet recyclable arrive dans le deleter
        if (objToDestroy.CompareTag("Waste Recycle"))
        {
            if (negativeDestroyEffect != null)
            {
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.TakeDamage(1);
                }

                // L'effet suivra la rotation indiquée de l'objet Deleter
                GameObject errorFx = Instantiate(negativeDestroyEffect, transform.position, transform.rotation * DeleterParticlesDirection);

                // Nettoyage automatique
                DestroyParticleAfterPlay(errorFx);
            }
        }

        // Bombe
        else if (objToDestroy.CompareTag("Bomb"))
        {

            if (bombDestroyEffect != null)
            {
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.TakeDamage(1, true); // on indique true pour dire que c'est une bombe, et arrêter directement la partie en cours
                }

                GameObject errorFx = Instantiate(bombDestroyEffect, transform.position, transform.rotation * DeleterParticlesDirection);

                // Nettoyage automatique
                DestroyParticleAfterPlay(errorFx);
            }
        }

            Destroy(objToDestroy); // détruire l'objet
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