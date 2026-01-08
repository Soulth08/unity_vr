using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager2 : MonoBehaviour
{
    public static GameManager2 Instance;

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

    public bool IsGameStarted => isGameStarted;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        currentHealth = maxHealth;
        isGameStarted = false;
        isGameOver = false;

        // --- ARRÊT INITIAL ---
        ActiverSystemes(false);

        UpdateUI();
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (startPanel != null) startPanel.SetActive(true);
    }

    // --- FONCTION DE DÉMARRAGE (Appelée par la poignée) ---
    public void StartGame()
    {
        if (isGameStarted || isGameOver) return;

        isGameStarted = true;

        // --- ACTIVATION DES SYSTÈMES ---
        ActiverSystemes(true);

        if (startPanel != null) startPanel.SetActive(false);
        Debug.Log("Systèmes activés : Tapis en marche et spawn lancé.");
    }

    private void ActiverSystemes(bool etat)
    {
        // On met les tapis en pause (ou on les relance)
        foreach (var tapis in tousLesTapis)
        {
            if (tapis != null) tapis.SetPaused(!etat);
        }

        // On désactive/active les scripts de spawn
        foreach (var lanceur in tousLesLanceurs)
        {
            if (lanceur != null) lanceur.enabled = etat;
        }
    }

    public void AddScore()
    {
        if (!isGameStarted || isGameOver) return;
        currentScore += scorePerWaste;
        UpdateUI();
    }

    public void TakeDamage(int damageAmount)
    {
        if (!isGameStarted || isGameOver) return;
        currentHealth -= damageAmount;
        if (currentHealth <= 0) TriggerGameOver();
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
        isGameStarted = false;
        ActiverSystemes(false); // On arrête tout en cas de défaite
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
    }
}