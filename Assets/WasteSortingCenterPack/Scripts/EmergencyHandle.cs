using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections;

public class EmergencyHandle : MonoBehaviour
{
    [SerializeField] private TreadmillsController[] treadmills; 

    [SerializeField] private float restartDelay = 2f;

    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable interactable;
    private bool isPulled = false;

    void Awake()
    {
        interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable>();
        if (interactable == null)
        {
            Debug.LogError("Error.");
        }
        if (treadmills == null || treadmills.Length == 0)
        {
            Debug.LogWarning("Warning.");
        }
    }

    void OnEnable()
    {
        if (interactable != null)
        {
            interactable.selectEntered.AddListener(OnHandlePulled);
            interactable.selectExited.AddListener(OnHandleReleased);
        }
    }

    void OnDisable()
    {
        if (interactable != null)
        {
            interactable.selectEntered.RemoveListener(OnHandlePulled);
            interactable.selectExited.RemoveListener(OnHandleReleased);
        }
    }

    public void OnHandlePulled(SelectEnterEventArgs args)
    {
        print("interacted");

        if (isPulled) return;
        isPulled = true;

        // Pause tous les tapis assignés
        foreach (var treadmill in treadmills)
        {
            treadmill.SetPaused(true);
        }
    }

    public void OnHandleReleased(SelectExitEventArgs args)
    {
        if (!isPulled) return;
        isPulled = false;

        StopAllCoroutines();
        StartCoroutine(RestartAfterDelay());
    }

    IEnumerator RestartAfterDelay()
    {
        yield return new WaitForSeconds(restartDelay);

        // Redémarre tous les tapis assignés
        foreach (var treadmill in treadmills)
        {
            treadmill.SetPaused(false);
        }
    }
}
