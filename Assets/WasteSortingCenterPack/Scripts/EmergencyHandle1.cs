using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections;
using UnityEngine.XR.Interaction.Toolkit.Interactables; 

public class EmergencyHandle : MonoBehaviour
{
    [SerializeField] private TreadmillsController[] treadmills; 

    [SerializeField] private float restartDelay = 2f;

    [Header("Clignotement")]
    [Tooltip("Fréquence de clignotement en secondes (ex: 0.5 clignotera 2 fois par seconde)")]
    [SerializeField] private float clignotementFrequence = 0.5f; 


    [Header("Indicateurs Lumineux")]
    public GameObject[] lumiereVertes; 
    public GameObject[] lumiereRouges; 

    private XRGrabInteractable grabInteractable;
    private bool isPulled = false; 

    void Awake()
    {

        grabInteractable = GetComponent<XRGrabInteractable>();

        if (grabInteractable == null)
        {
            return; 
        }

        if (treadmills == null || treadmills.Length == 0)
        {
            Debug.LogWarning("Aucun contrôleur de tapis assigné.");
        }
        if (lumiereVertes.Length != lumiereRouges.Length)
        {
            Debug.LogError("Erreur Configuration Lumières: le nombre de lumières Vertes et Rouges doit être identique.");
        }

        MettreAJourLumiere(true); 
    }

    void OnEnable()
    {
        // Abonne les fonctions aux événements de sélection (saisie)
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.AddListener(OnHandlePulled);
            grabInteractable.selectExited.AddListener(OnHandleReleased);
        }
    }

    void OnDisable()
    {
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.RemoveListener(OnHandlePulled);
            grabInteractable.selectExited.RemoveListener(OnHandleReleased);
        }
    }

    // Gestion de l'interaction 
    public void OnHandlePulled(SelectEnterEventArgs args)
    {
        if (isPulled) return;
        isPulled = true;

        StopAllCoroutines(); 
        
        foreach (var treadmill in treadmills)
        {
            treadmill.SetPaused(true);
        }

        StartCoroutine(ClignotementLumiereRouge()); 
        
        print("Poignée d'urgence tirée. Tapis à l'arrêt. Lumière ROUGE clignotante.");
    }
    public void OnHandleReleased(SelectExitEventArgs args)
    {
        if (!isPulled) return;

        StopCoroutine(ClignotementLumiereRouge());
        
        StartCoroutine(RestartAfterDelay());
    }
    
    // Clignotement 
    IEnumerator ClignotementLumiereRouge()
    {
        MettreAJourLumiere(false); 

        while (isPulled) 
        {
            // Allume les lumières rouges
            for (int i = 0; i < lumiereRouges.Length; i++)
            {
                if (lumiereRouges[i] != null) lumiereRouges[i].SetActive(true);
            }
            yield return new WaitForSeconds(clignotementFrequence); 

            // Éteint les lumières rouges
            for (int i = 0; i < lumiereRouges.Length; i++)
            {
                if (lumiereRouges[i] != null) lumiereRouges[i].SetActive(false);
            }
            yield return new WaitForSeconds(clignotementFrequence); 
        }

        for (int i = 0; i < lumiereRouges.Length; i++)
        {
             if (lumiereRouges[i] != null) lumiereRouges[i].SetActive(false);
        }
    }
    
    // Logique d'Arrêt / Redémarrage Différé 
    IEnumerator RestartAfterDelay()
    {
        yield return new WaitForSeconds(restartDelay);

        StopCoroutine(ClignotementLumiereRouge()); 

        foreach (var treadmill in treadmills)
        {
            treadmill.SetPaused(false);
        }

        isPulled = false; 
        MettreAJourLumiere(true);
        
        print("Redémarrage effectué. Lumière Verte active.");
    }

    private void MettreAJourLumiere(bool estEnMarche)
    {
        if (lumiereVertes.Length != lumiereRouges.Length)
        {
            return; 
        }

        for (int i = 0; i < lumiereVertes.Length; i++)
        {
            if (lumiereVertes[i] != null && lumiereRouges[i] != null)
            {
                lumiereVertes[i].SetActive(estEnMarche);
                lumiereRouges[i].SetActive(!estEnMarche); 
            }
        }
    }
}