using UnityEngine;
using TMPro; // Necessaire pour grer le texte UI
using UnityEngine.SceneManagement; // Pour relancer la scne si Game Over

public class GameManager : MonoBehaviour
{
    // Le Singleton : permet d'accder  "GameManager.Instance" depuis n'importe quel script
    public static GameManager Instance;

    [Header("Paramtres de Jeu")]
    public int maxHealth = 3;
    public int scorePerWaste = 1;

    [Header("Interface (UI)")]
    public TMP_Text scoreText;   // Assigne ton texte de Score ici
    public TMP_Text healthText;  // Assigne ton texte de Vie ici
    public GameObject gameOverPanel; // Optionnel : un panneau "Perdu"

    // Variables prives
    private int currentScore = 0;
    private int currentHealth;
    private bool isGameOver = false;

    private void Awake()
    {
        // Initialisation du Singleton
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        currentHealth = maxHealth;
        UpdateUI();

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }

    // Fonction appele pour gagner des points
    public void AddScore()
    {
        if (isGameOver) return;

        currentScore += scorePerWaste;
        UpdateUI();
    }

    // Fonction appele pour perdre de la vie
    public void TakeDamage(int damageAmount)
    {
        if (isGameOver) return;

        currentHealth -= damageAmount;

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            TriggerGameOver();
        }

        UpdateUI();
    }

    private void UpdateUI()
    {
        if (scoreText != null) scoreText.text = "Score: " + currentScore;
        if (healthText != null) healthText.text = "Vies: " + currentHealth;
    }

    private void TriggerGameOver()
    {
        isGameOver = true;
        Debug.Log("GAME OVER");

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        // Ici tu pourras ajouter un bouton pour relancer la scne
        // SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}