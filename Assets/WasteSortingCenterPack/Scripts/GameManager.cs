using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // Le Singleton : permet d'accéder à "GameManager.Instance" depuis n'importe quel script
    public static GameManager Instance;

    [Header("Paramètres de Jeu")]
    public int maxHealth = 3;
    public int scorePerWaste = 1;

    [Header("Interface (UI)")]
    public TMP_Text scoreText;
    public TMP_Text healthText;
    public GameObject gameOverPanel;
    public GameObject startPanel; // Panneau "Tirez pour commencer"

    [Header("Contrôle des Systèmes")]
    [Tooltip("Glisse tes scripts de tapis ici")]
    public TreadmillsController[] tousLesTapis;

    [Tooltip("Glisse tes scripts de spawn de déchets ici")]
    public MonoBehaviour[] tousLesLanceurs;

    // Variables d'état
    private int currentScore = 0;
    private int currentHealth;
    private bool isGameOver = false;
    private bool isGameStarted = false;

    // Propriété publique en lecture seule pour vérifier si la partie a commencé
    public bool IsGameStarted => isGameStarted;

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
        isGameStarted = false;
        isGameOver = false;

        // --- ARRÊT INITIAL : Tout est désactivé avant que la poignée soit tirée ---
        ActiverSystemes(false);

        UpdateUI();

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (startPanel != null)
            startPanel.SetActive(true);
    }

    /// <summary>
    /// Fonction appelée par la StartHandle pour démarrer la partie
    /// </summary>
    public void StartGame()
    {
        // Évite de démarrer plusieurs fois ou si la partie est terminée
        if (isGameStarted || isGameOver)
        {
            Debug.LogWarning("La partie a déjà commencé ou est terminée !");
            return;
        }

        isGameStarted = true;

        // --- ACTIVATION DES SYSTÈMES ---
        ActiverSystemes(true);

        // Cache le panneau de démarrage
        if (startPanel != null)
            startPanel.SetActive(false);

        Debug.Log("🚀 PARTIE DÉMARRÉE : Tapis en marche et spawn lancé !");
    }

    /// <summary>
    /// Active ou désactive tous les systèmes du jeu (tapis + spawners)
    /// </summary>
    private void ActiverSystemes(bool etat)
    {
        // Gestion des tapis : si etat = true → on démarre (SetPaused = false)
        foreach (var tapis in tousLesTapis)
        {
            if (tapis != null)
                tapis.SetPaused(!etat);
        }

        // Gestion des spawners : on active/désactive les scripts
        foreach (var lanceur in tousLesLanceurs)
        {
            if (lanceur != null)
                lanceur.enabled = etat;
        }
    }

    /// <summary>
    /// Fonction appelée pour gagner des points. par défaut à scoreperwaste, mais peut prendre un autre nombre
    /// </summary>
    public void AddScore(int customScorePerWaste = 0)
    {
        if (customScorePerWaste != 0) scorePerWaste = customScorePerWaste;
        if (!isGameStarted || isGameOver) return;

        currentScore += scorePerWaste;
        UpdateUI();
    }

    /// <summary>
    /// Fonction appelée pour perdre de la vie
    /// </summary>
    public void TakeDamage(int damageAmount)
    {
        if (!isGameStarted || isGameOver) return;

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
        if (scoreText != null)
            scoreText.text = "Score: " + currentScore;

        if (healthText != null)
            healthText.text = "Vies: " + currentHealth;
    }

    private void TriggerGameOver()
    {
        isGameOver = true;
        isGameStarted = false;

        // On arrête tous les systèmes en cas de défaite
        ActiverSystemes(false);

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        Debug.Log("💀 GAME OVER");
    }

    /// <summary>
    /// Fonction pour relancer la partie (à appeler depuis un bouton UI)
    /// </summary>
    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}