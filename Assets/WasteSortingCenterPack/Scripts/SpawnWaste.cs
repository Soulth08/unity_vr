using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class WasteSpawnItem
{
    public GameObject prefab;
    [Range(0f, 100f)]
    public float probability = 10f;
}

public class SpawnWaste : MonoBehaviour
{
    [Header("Paramètres de Spawn")]
    public Vector3 PositionSpawn;
    public Vector3 ForceSpawn;
    public float minForce = 1f;
    public float maxForce = 2f;
    public float minTimeBetweenSpawn = 1f;
    public float maxTimeBetweenSpawn = 3f;

    public List<WasteSpawnItem> wasteItems = new List<WasteSpawnItem>();

    [Header("Difficulté Progressive")]
    [Tooltip("Temps en secondes avant que la difficulté commence à augmenter")]
    public float tempsAvantDifficulté = 30f;

    [Tooltip("Combien de secondes on retire au délai de spawn par seconde de jeu")]
    public float accelerationParSeconde = 0.05f;

    [Tooltip("limite absolue : on ne pourra jamais spawner plus vite que ça")]
    public float limiteMinimaleAbsolue = 0.5f;

    // Variables internes
    private float nextSpawnTime;
    private float timeStarted; // quand le spawner a été activé, permet de suivre l'évolution du jeu
    private float baseMinTime;
    private float baseMaxTime;

    //son de spawn
    private AudioSource audioSource;
    public AudioClip spawnSound;

    void Awake()
    {
        // On sauvegarde les valeurs configurées dans l'inspecteur
        baseMinTime = minTimeBetweenSpawn;
        baseMaxTime = maxTimeBetweenSpawn;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    void OnEnable()
    {
        // On enregistre le moment de début dès que le script est activé par le GameManager
        timeStarted = Time.time;
        SetNextSpawnTime();
    }

    void Update()
    {
        if (Time.time >= nextSpawnTime)
        {
            SpawnRandomWaste(); // Spawn un objet
            SetNextSpawnTime(); // Met à jour la difficulté
        }
    }

    void SetNextSpawnTime()
    {
        //  temps écoulé depuis l'activation du spawner
        float tempsEcoule = Time.time - timeStarted;

        float currentMin = baseMinTime;
        float currentMax = baseMaxTime;

        // ne pas augmenter la difficulté avant un peu de temps
        if (tempsEcoule > tempsAvantDifficulté)
        {
            // On calcule combien de temps on a passé dans la phase de difficulté
            float tempsDeDifficulté = tempsEcoule - tempsAvantDifficulté;

            // difficulté linéaire (on aurait aussi pu mettre logaritmique)
            float reduction = tempsDeDifficulté * accelerationParSeconde;

            // ne peut pas descendre sous la limite absolue
            currentMin = Mathf.Max(limiteMinimaleAbsolue, baseMinTime - reduction);
            currentMax = Mathf.Max(limiteMinimaleAbsolue, baseMaxTime - reduction);
        }

        // prochain spawn avec les nouvelles valeurs
        // Mathf.Max(currentMin, currentMax) assure que le max n'est jamais inférieur au min
        float randomDelay = Random.Range(currentMin, Mathf.Max(currentMin, currentMax));

        nextSpawnTime = Time.time + randomDelay;
    }

    void SpawnRandomWaste()
    {
        if (wasteItems.Count == 0) return; // si aucun objet dans la liste, alors on ne peut rien faire

        // calcul de la somme totale, utilisé pour avoir la probabilité de chaque objet
        float totalProbability = 0f;
        foreach (var item in wasteItems)
        {
            if (item.prefab != null) totalProbability += item.probability;
        }

        if (totalProbability <= 0) return;


        float randomValue = Random.Range(0f, totalProbability);
        float cumulativeProbability = 0f;

        foreach (var item in wasteItems)
        {
            if (item.prefab != null)
            {
                cumulativeProbability += item.probability;
                if (randomValue <= cumulativeProbability)
                {
                    GameObject spawnedObject = Instantiate(item.prefab, transform.position + PositionSpawn, RandomRotation());

                    Rigidbody rb = spawnedObject.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        rb.AddForce(ForceSpawn * RandomForce(), ForceMode.Impulse);
                    }

                    if (spawnSound != null && audioSource != null)
                    {
                        // PlayOneShot permet de jouer le son sans couper un autre son éventuel
                        audioSource.PlayOneShot(spawnSound);
                    }

                    return;
                }
            }
        }
    }

    float RandomForce()
    {
        return Random.Range(minForce, maxForce);
    }

    Quaternion RandomRotation()
    {
        return Quaternion.Euler(
            Random.Range(0f, 360f),
            Random.Range(0f, 360f),
            Random.Range(0f, 360f)
        );
    }
}