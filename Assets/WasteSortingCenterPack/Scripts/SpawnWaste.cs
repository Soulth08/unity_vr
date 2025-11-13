using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UIElements;

[System.Serializable]
public class WasteSpawnItem
{
    public GameObject prefab;
    [Range(0f, 100f)]
    public float probability = 10f;
}

public class SpawnWaste : MonoBehaviour
{
    public Vector3 PositionSpawn;
    public Vector3 ForceSpawn;
    public float minForce =0f;
    public float maxForce =10f;

    public List<WasteSpawnItem> wasteItems = new List<WasteSpawnItem>();
    public float minTimeBetweenSpawn = 1f;
    public float maxTimeBetweenSpawn = 3f;

    private float nextSpawnTime;

    void Start()
    {
        SetNextSpawnTime();
    }

    void Update()
    {
        if (Time.time >= nextSpawnTime)
        {
            SpawnRandomWaste();
            SetNextSpawnTime();
        }
    }

    void SpawnRandomWaste()
    {
        if (wasteItems.Count == 0)
        {
            Debug.LogWarning("Aucun déchet dans la liste!");
            return;
        }

        // Calculer la somme totale des probabilités
        float totalProbability = 0f;
        foreach (var item in wasteItems)
        {
            if (item.prefab != null)
            {
                totalProbability += item.probability;
            }
        }

        if (totalProbability <= 0)
        {
            Debug.LogWarning("La probabilité totale est 0!");
            return;
        }

        // Générer un nombre aléatoire entre 0 et la probabilité totale
        float randomValue = Random.Range(0f, totalProbability);

        // Sélectionner le déchet en fonction de la probabilité
        float cumulativeProbability = 0f;
        foreach (var item in wasteItems)
        {
            if (item.prefab != null)
            {
                cumulativeProbability += item.probability;
                if (randomValue <= cumulativeProbability)
                {
                    GameObject spawnedObject = Instantiate(item.prefab, transform.position + PositionSpawn, RandomRotation());

                    // Appliquer une force si l'objet a un Rigidbody
                    Rigidbody rb = spawnedObject.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        rb.AddForce(ForceSpawn*RandomForce(), ForceMode.Impulse);
                    }

                    return;
                }
            }
        }
    }

    void SetNextSpawnTime()
    {
        float randomDelay = Random.Range(minTimeBetweenSpawn, maxTimeBetweenSpawn);
        nextSpawnTime = Time.time + randomDelay;
    }

    float RandomForce()
    {
        return Random.Range(minForce, maxForce);
    }

    Quaternion RandomRotation()
    {
        return Quaternion.Euler(
            Random.Range(0f, 360f),  // Rotation X
            Random.Range(0f, 360f),  // Rotation Y
            Random.Range(0f, 360f)   // Rotation Z
        );
    }

}