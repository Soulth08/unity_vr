using UnityEngine;

public class Pull_handle : MonoBehaviour
{
    [Header("Paramètres")]
    [Tooltip("La hauteur à laquelle le cube va monter en mètres.")]
    public float hauteurMontee = 1.5f;
    public float vitesseMontee = 2.0f;
  

    private bool estActive = false;
    private Vector3 positionInitiale;
    private Vector3 positionCible;

    void Start()
    {
        // On mémorise la position de départ
        positionInitiale = transform.position;
        // On calcule où il doit aller
        positionCible = positionInitiale + Vector3.up * hauteurMontee;
    }

    public void ActiverLeMouvement()
    {
        if (!estActive)
        {
            Debug.Log("piognéééé monte");
            estActive = true;
        }
    }


    void Update()
    {
        // Si le mouvement est activé
        if (estActive)
        {
            //Monter vers la cible

            transform.position = Vector3.MoveTowards(transform.position, positionCible, vitesseMontee * Time.deltaTime);


        }
    }
}