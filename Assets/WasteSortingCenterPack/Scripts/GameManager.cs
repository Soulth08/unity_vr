using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    // --- NOUVEAU : Cette variable survit au rechargement de la scène ---
    private static bool autoStartNextTime = false;

    [Header("Paramètres de Jeu")]
    public int maxHealth = 3;
    public int scorePerWaste = 1;

    [Header("Interface Petit Écran (Machine)")]
    public TMP_Text machineScreenText;

    [Header("Messages Écran")]
    [TextArea] public string txtMenu = "TIRER POUR\nCOMMENCER";
    public string txtPret = "PRÊT...";
    public string txtJeu = "REINITIALISER";
    public string txtGameOver = "REJOUER";

    [Header("Interface Monde")]
    public TMP_Text countdownText;
    public TMP_Text scoreText;
    public TMP_Text highScoreText;
    public TMP_Text healthText;
    public GameObject gameOverPanel;

    [Header("Audio")]
    public AudioClip gameOverSound;
    public AudioClip goSound;
    public AudioClip scoreSound;
    public AudioClip damageSound;
    public AudioClip bombSound;
    private AudioSource audioSource;

    [Header("Systèmes")]
    public TreadmillsController[] tousLesTapis;
    public MonoBehaviour[] tousLesLanceurs;

    // État
    private int currentScore = 0;
    private int highScore = 0;
    private int currentHealth;
    public bool isGameOver = false;
    private bool isGameStarted = false;

    private const string PREF_HIGHSCORE = "BestScoreKey";
    private Coroutine blinkingCoroutine;

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

        // Setup Audio
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        highScore = PlayerPrefs.GetInt(PREF_HIGHSCORE, 0);

        ActiverSystemes(false);

        if (countdownText != null) countdownText.gameObject.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (machineScreenText != null) machineScreenText.gameObject.SetActive(true);

        UpdateUI();

        // --- MODIFICATION ICI ---
        // On vérifie si on vient d'un "Rejouer"
        if (autoStartNextTime)
        {
            // C'est un redémarrage automatique
            autoStartNextTime = false; // On remet à zéro pour la prochaine fois
            StartCoroutine(SequenceDemarrage()); // On lance direct le décompte !
        }
        else
        {
            // C'est un lancement normal du jeu (depuis le bureau du casque)
            SetMachineText(txtMenu, true);
        }
    }

    public void OnHandleAction()
    {
        if (!isGameStarted && !isGameOver)
        {
            StartCoroutine(SequenceDemarrage());
        }
        else
        {
            RestartGame();
        }
    }

    private IEnumerator SequenceDemarrage()
    {
        SetMachineText(txtPret, false);

        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(true);

            countdownText.text = "3";
            yield return new WaitForSeconds(1f);
            countdownText.text = "2";
            yield return new WaitForSeconds(1f);
            countdownText.text = "1";
            yield return new WaitForSeconds(1f);

            countdownText.text = "GO !";
            if (goSound != null && audioSource != null) audioSource.PlayOneShot(goSound);

            yield return new WaitForSeconds(0.5f);
            countdownText.gameObject.SetActive(false);
        }
        else
        {
            yield return new WaitForSeconds(1f);
        }

        LancerGameplay();
    }

    private void LancerGameplay()
    {
        isGameStarted = true;
        currentScore = 0;
        UpdateUI();

        ActiverSystemes(true);
        SetMachineText(txtJeu, false);
        Debug.Log("🚀 JEU LANCÉ");
    }

    public void RestartGame()
    {
        // --- MODIFICATION ICI ---
        // On signale qu'on veut démarrer tout de suite au prochain chargement
        autoStartNextTime = true;

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void TriggerGameOver()
    {
        isGameOver = true;
        isGameStarted = false;
        ActiverSystemes(false);

        if (gameOverSound != null && audioSource != null) audioSource.PlayOneShot(gameOverSound);

        if (currentScore > highScore)
        {
            highScore = currentScore;
            PlayerPrefs.SetInt(PREF_HIGHSCORE, highScore);
            PlayerPrefs.Save();
            Debug.Log("Nouveau record enregistré !");
        }

        UpdateUI();

        if (gameOverPanel != null) gameOverPanel.SetActive(true);

        SetMachineText(txtGameOver, true);
        Debug.Log("💀 GAME OVER");
    }

    // ... LE RESTE DU CODE (UI, ActiverSystemes, Scores, TakeDamage) EST IDENTIQUE ...

    private void SetMachineText(string message, bool clignote)
    {
        if (machineScreenText == null) return;
        if (blinkingCoroutine != null) StopCoroutine(blinkingCoroutine);

        machineScreenText.text = message;
        machineScreenText.enabled = true;

        if (clignote) blinkingCoroutine = StartCoroutine(RoutineClignotement());
    }

    private IEnumerator RoutineClignotement()
    {
        if (machineScreenText != null) machineScreenText.enabled = true;
        while (true)
        {
            yield return new WaitForSeconds(0.6f);
            if (machineScreenText != null) machineScreenText.enabled = !machineScreenText.enabled;
            else yield break;
        }
    }

    private void ActiverSystemes(bool etat)
    {
        foreach (var t in tousLesTapis) if (t != null) t.SetPaused(!etat);
        foreach (var l in tousLesLanceurs) if (l != null) l.enabled = etat;
    }

    public void AddScore(int amount = 1)
    {
        if (!isGameStarted || isGameOver) return;
        currentScore += amount;
        UpdateUI();

        if (scoreSound != null && audioSource != null) audioSource.PlayOneShot(scoreSound);
    }

    public void TakeDamage(int damage = 1, bool isBomb = false)
    {
        if (!isGameStarted || isGameOver) return;

        if (isBomb) currentHealth = 0;
        else currentHealth -= damage;

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            TriggerGameOver();
        }
        UpdateUI();

        if (damageSound != null && audioSource != null && !isBomb)
        {
            audioSource.PlayOneShot(damageSound);
        }
        else if (bombSound != null && audioSource != null && isBomb)
        {
            audioSource.PlayOneShot(bombSound);
        }
    }

    /// <summary>
    /// Ajoute une vie au joueur (sans dépasser le max)
    /// </summary>
    public void AddLife(int amount = 1)
    {
        if (!isGameStarted || isGameOver) return;

        currentHealth += amount;

        // On s'assure de ne pas dépasser le maximum de vie défini
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }

        UpdateUI();

        // Optionnel : Jouer un son de "Soin" ou "PowerUp" ici
        // if (audioSource != null && healSound != null) audioSource.PlayOneShot(healSound);
    }

    private void UpdateUI()
    {
        if (scoreText != null) scoreText.text = "Score: " + currentScore;
        if (healthText != null) healthText.text = "Vies: " + currentHealth;
        if (highScoreText != null) highScoreText.text = "Record: " + highScore;
    }
}