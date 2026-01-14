using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    // permet de redemarrer directement lorsque l'on rejoue, sans devoir tirer la poignée à nouveau
    private static bool autoStartNextTime = false;

    [Header("Paramètres de Jeu")]
    public int maxHealth = 5;
    public int scorePerWaste = 1;

    [Header("Messages sur Ordinateur")]
    public TMP_Text machineScreenText;
    public string txtStart = "TIRER POUR COMMENCER";
    public string txtReady = "PRÊT...";
    public string txtInGame = "REINITIALISER";
    public string txtPlayAgain = "REJOUER";

    [Header("Interface Monde")]
    public TMP_Text countdownText;
    public TMP_Text scoreText;
    public TMP_Text highScoreText;
    public TMP_Text healthText;
    public GameObject gameOverPanel;

    [Header("Audio")]
    public AudioClip gameOverSound;
    public AudioClip startSound;
    public AudioClip scoreSound;
    public AudioClip damageSound;
    public AudioClip bombSound;
    public AudioClip healSound;
    private AudioSource audioSource;

    [Header("Systèmes")]
    public TreadmillsController[] Treadmills;
    public MonoBehaviour[] Spawner;

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
        // évite qu'il y ait plus d'une instance
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // Initialisation
        currentHealth = maxHealth;
        isGameStarted = false;
        isGameOver = false;

        // Setup Audio
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        // Setup HighScore depuis la sauvegarde
        highScore = PlayerPrefs.GetInt(PREF_HIGHSCORE, 0);

        // Arrêter tous les systèmes (treadmills, spawner...)
        SystemsActivated(false);

        // Initialisation de l'interface
        if (countdownText != null) countdownText.gameObject.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (machineScreenText != null) machineScreenText.gameObject.SetActive(true);
        UpdateUI();

        // On vérifie si on vient d'un "Rejouer"
        if (autoStartNextTime)
        {
            // C'est un redémarrage automatique
            autoStartNextTime = false; // On remet à zéro pour la prochaine fois
            StartCoroutine(StartSequence()); // On lance direct le décompte !
        }
        else
        {
            // C'est un lancement normal du jeu
            SetMachineText(txtStart, true);
        }
    }


    // PullHandle pour démarrer le jeu ou rejouer
    public void StartHandle()
    {
        if (!isGameStarted && !isGameOver)
        {
            StartCoroutine(StartSequence());
        }
        else
        {
            RestartGame();
        }
    }

    // Lance le countdown avant le lancement du jeu
    private IEnumerator StartSequence()
    {
        SetMachineText(txtReady, false);

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
            if (startSound != null && audioSource != null) audioSource.PlayOneShot(startSound);

            yield return new WaitForSeconds(0.5f);
            countdownText.gameObject.SetActive(false);
        }
        else
        {
            yield return new WaitForSeconds(1f);
        }

        StartGameplay();
    }


    // Lance le jeu
    private void StartGameplay()
    {
        isGameStarted = true; // On indique que le jeu a commencé
        currentScore = 0; // On remet le score à zéro
        UpdateUI(); // On met à jour l'interface, important d ele faire à chaque modification des valeurs du jeu !

        SystemsActivated(true);
        SetMachineText(txtInGame, false);
        Debug.Log("🚀 JEU LANCÉ");
    }


    // Rejouer
    public void RestartGame()
    {
        // On indique qu'on veut démarrer tout de suite au prochain chargement, sans le countdown
        autoStartNextTime = true;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); // On redémarre la scène avec ce nouveau paramètre
    }


    // GameOver
    private void TriggerGameOver()
    {
        isGameOver = true;
        isGameStarted = false;
        SystemsActivated(false); // On arrête tous les systèmes

        // On joue le son de GameOver
        if (gameOverSound != null && audioSource != null) audioSource.PlayOneShot(gameOverSound);


        // On sauvegarde le meilleur score en local
        // devrait fonctionner sur toutes les plateformes parce que c'est un système standard de Unity
        if (currentScore > highScore)
        {
            highScore = currentScore;
            PlayerPrefs.SetInt(PREF_HIGHSCORE, highScore);
            PlayerPrefs.Save();
            Debug.Log("Nouveau record enregistré !");
        }

        UpdateUI();
        SetMachineText(txtPlayAgain, true);

        // On affiche le texte de GameOver
        if (gameOverPanel != null) gameOverPanel.SetActive(true);

        Debug.Log("💀 GAME OVER");
    }


    // Affiche un message sur l'interface de l'ordinateur à côté du joueur
    private void SetMachineText(string message, bool clignote)
    {
        if (machineScreenText == null) return;
        if (blinkingCoroutine != null) StopCoroutine(blinkingCoroutine);

        machineScreenText.text = message;
        machineScreenText.enabled = true;

        if (clignote) blinkingCoroutine = StartCoroutine(RoutineBlinking()); // Le texte clignote pour attirer l'attention
    }

    private IEnumerator RoutineBlinking()
    {
        if (machineScreenText != null) machineScreenText.enabled = true;

        // Clignotement
        while (true)
        {
            yield return new WaitForSeconds(0.6f);
            if (machineScreenText != null) machineScreenText.enabled = !machineScreenText.enabled; // cette condition est vraie 50% du temps, donc le texte clignote
            else yield break;
        }
    }


    // Définit si les systèmes sont activés ou non, utilisé lors du GameOver ou avant le lancement du jeu
    private void SystemsActivated(bool etat)
    {
        foreach (var t in Treadmills) if (t != null) t.SetPaused(!etat);
        foreach (var l in Spawner) if (l != null) l.enabled = etat;
    }


    //Ajoute un score, peut avoir des valeurs négatives si on est vraiment trop nul
    public void AddScore(int amount = 1)
    {
        if (!isGameStarted || isGameOver) return; // On ne peut pas ajouter de score si le jeu n'a pas commencé
        currentScore += amount;
        UpdateUI();

        if (scoreSound != null && audioSource != null) audioSource.PlayOneShot(scoreSound);
    }

    //Ajoute un score, peut avoir des valeurs négatives si on est vraiment trop nul
    public void RemoveScore(int amount = 1)
    {
        if (!isGameStarted || isGameOver) return; // On ne peut pas retirer de score si le jeu n'a pas commencé
        currentScore -= amount;
        UpdateUI();

        if (scoreSound != null && audioSource != null) audioSource.PlayOneShot(damageSound);
    }


    // Gère les vies du joueur
    public void TakeDamage(int damage = 1, bool isBomb = false)
    {
        if (!isGameStarted || isGameOver) return; // On ne peut pas prendre de dommages si le jeu n'a pas commencé

        if (isBomb) currentHealth = 0; // Si c'est une bombe, on perd toutes les vies d'un seul coup
        else currentHealth -= damage; // Sinon, on perd une seule vie


        // On s'assure de ne pas avoir de vie négative
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            TriggerGameOver();
        }
        UpdateUI();


        // Joue un son de "Dommage" ou de "Explosion"
        if (damageSound != null && audioSource != null && !isBomb)
        {
            audioSource.PlayOneShot(damageSound);
        }
        else if (bombSound != null && audioSource != null && isBomb)
        {
            audioSource.PlayOneShot(bombSound);
        }
    }

    // Ajoute une vie au joueur (sans dépasser le max)
    public void AddLife(int amount = 1)
    {
        if (!isGameStarted || isGameOver) return;

        currentHealth += amount;
        UpdateUI();

        // Il n'y a pas de limite positive de vie, mais il serait tout à fait possible de l'ajouter
        // On a préféré faire de cette manière pour récompenser le joueur dans le cas où il aurait beaucoup de bombes à gérer
        // Chaque bombe lui donne une vie supplementaire

        if (audioSource != null && healSound != null) audioSource.PlayOneShot(healSound);
    }


    // Mise à jour de l'interface, à faire chaque fois qu'on modifie les valeurs du jeu
    private void UpdateUI()
    {
        if (scoreText != null) scoreText.text = "Score: " + currentScore;
        if (healthText != null) healthText.text = "Vies: " + currentHealth;
        if (highScoreText != null) highScoreText.text = "Record: " + highScore;
    }
}