using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections;

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

    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable interactable;
    private bool isPulled = false; 

    void Awake()
    {
        interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable>();

        if (treadmills == null || treadmills.Length == 0)
        {
            Debug.LogWarning("Aucun contrôleur de tapis assigné.");
        }
        if (lumiereVertes.Length != lumiereRouges.Length)
        {
            Debug.LogError("Erreur Configuration Lumières: Le nombre de lumières Vertes et Rouges doit être identique!");
        }

        MettreAJourLumiere(true); 
    }

    void OnEnable()
    {
        if (interactable != null)
        {
            interactable.selectEntered.AddListener(args => OnHandlePulled());
            interactable.selectExited.AddListener(args => OnHandleReleased());
        }
    }

    void OnDisable()
    {
        if (interactable != null)
        {
            interactable.selectEntered.RemoveAllListeners();
            interactable.selectExited.RemoveAllListeners();
        }
    }

    // Avec la souris
    void OnMouseDown()
    {
        OnHandlePulled();
    }
    
    void OnMouseUp()
    {
        OnHandleReleased();
    }

    // Gestion de l'interaction 

    public void OnHandlePulled(SelectEnterEventArgs args = null)
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

    public void OnHandleReleased(SelectExitEventArgs args = null)
    {
        if (!isPulled) return;

        StopCoroutine(ClignotementLumiereRouge());
        StartCoroutine(ClignotementLumiereRouge()); 
        
        StartCoroutine(RestartAfterDelay());
    }
    
    // Clignotement 
    IEnumerator ClignotementLumiereRouge()
    {
        MettreAJourLumiere(false);

        while (isPulled) 
        {
            for (int i = 0; i < lumiereRouges.Length; i++)
            {
                if (lumiereRouges[i] != null) lumiereRouges[i].SetActive(true);
            }
            yield return new WaitForSeconds(clignotementFrequence); 

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
        
        print("Lumière Verte active.");
    }
    
    // Affichage des Lumières 
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