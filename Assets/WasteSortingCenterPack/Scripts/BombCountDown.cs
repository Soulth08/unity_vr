using UnityEngine;
using TMPro;

public class BombCountdown : MonoBehaviour
{
    [Header("Paramètres Temps")]
    public float tempsAvantExplosion = 20f;
    public float vitesseClignotementStart = 0.1f;
    public float vitesseClignotementEnd = 2f;

    [Header("Feedback Visuel")]
    public Renderer bombRenderer;
    [ColorUsage(true, true)]
    public Color couleurFlash = Color.white;
    public GameObject bombDestroyEffect;

    [Header("Feedback Audio - Explosion")]
    public AudioClip explosionSound;
    [Range(0f, 1f)] public float volumeExplosion = 1f;
    [Tooltip("0 = 2D, 1 = 3D")]
    [Range(0f, 1f)] public float spatialBlend = 1f;
    public float minPitch = 0.8f;
    public float maxPitch = 1.2f;

    [Header("Feedback Audio - Timer (Tic-Tac)")]
    [Tooltip("Le son du compte à rebours (Mèche, Bip-bip...). La fin du fichier sera synchronisée avec l'explosion.")]
    public AudioClip timerSound; // <--- NOUVEAU
    [Range(0f, 1f)] public float volumeTimer = 1f; // <--- NOUVEAU

    [Header("Feedback UI")]
    public TMP_Text timerText;

    // Variables internes
    private float timerActuel;
    private bool aExplose = false;
    private int emissionColorId;

    private AudioSource timerAudioSource; // La source qui joue le tic-tac sur la bombe
    private bool timerSoundStarted = false;

    private void Start()
    {
        timerActuel = tempsAvantExplosion;

        if (bombRenderer == null) bombRenderer = GetComponentInChildren<Renderer>();

        emissionColorId = Shader.PropertyToID("_EmissionColor");
        if (bombRenderer != null) bombRenderer.material.EnableKeyword("_EMISSION");

        // --- SETUP AUDIO TIMER ---
        if (timerSound != null)
        {
            // On ajoute un AudioSource directement sur la bombe pour qu'il la suive
            timerAudioSource = gameObject.AddComponent<AudioSource>();
            timerAudioSource.clip = timerSound;
            timerAudioSource.volume = volumeTimer;
            timerAudioSource.spatialBlend = 1f; // Toujours en 3D pour entendre d'où vient la menace
            timerAudioSource.loop = false; // Important : pas de boucle, on veut la fin précise
            timerAudioSource.playOnAwake = false;

            // LOGIQUE DE SYNCHRONISATION
            // Cas 1 : Le son est plus long que le timer (ex: Son 30s, Timer 20s)
            // On doit jouer immédiatement, mais en sautant le début pour ne jouer que la fin.
            if (timerSound.length >= tempsAvantExplosion)
            {
                timerAudioSource.time = timerSound.length - tempsAvantExplosion;
                timerAudioSource.Play();
                timerSoundStarted = true;
            }
            // Cas 2 : Le son est plus court (ex: Son 5s, Timer 20s)
            // On ne fait rien ici, on attendra dans l'Update.
        }
    }

    private void Update()
    {
        if (GameManager.Instance.isGameOver || aExplose) return;

        timerActuel -= Time.deltaTime;

        // --- GESTION AUDIO TIMER (Cas du son court) ---
        if (timerAudioSource != null && !timerSoundStarted)
        {
            // Si le temps restant est inférieur ou égal à la durée du son, on lance !
            if (timerActuel <= timerSound.length)
            {
                timerAudioSource.Play();
                timerSoundStarted = true;
            }
        }
        // ----------------------------------------------

        if (timerText != null)
        {
            timerText.text = timerActuel.ToString("F0");
            if (timerActuel <= 5) timerText.color = Color.red;
        }

        if (bombRenderer != null) ApplyBlinkingEffect();

        if (timerActuel <= 0) Explode();
    }

    private void ApplyBlinkingEffect()
    {
        float progression = 1 - (timerActuel / tempsAvantExplosion);
        float vitesseActuelle = Mathf.Lerp(vitesseClignotementStart, vitesseClignotementEnd, progression);
        float lerp = (Mathf.Sin(Time.time * vitesseActuelle) + 1f) / 2f;

        Color finalEmission = Color.Lerp(Color.black, couleurFlash, lerp);
        bombRenderer.material.SetColor(emissionColorId, finalEmission);
    }

    private void Explode()
    {
        aExplose = true;
        Debug.Log("BOUM !");

        if (GameManager.Instance != null) GameManager.Instance.TakeDamage(1, true);

        if (bombDestroyEffect != null)
            Instantiate(bombDestroyEffect, transform.position, transform.rotation);

        if (explosionSound != null)
        {
            PlayCustomSound();
        }

        // En détruisant l'objet, l'AudioSource du timer attaché dessus sera coupé net.
        // C'est exactement ce qu'on veut (le tic-tac s'arrête quand ça explose).
        Destroy(gameObject);
    }

    private void PlayCustomSound()
    {
        GameObject audioObj = new GameObject("TempExplosionAudio");
        audioObj.transform.position = transform.position;

        AudioSource source = audioObj.AddComponent<AudioSource>();
        source.clip = explosionSound;
        source.volume = volumeExplosion;
        source.spatialBlend = spatialBlend;

        source.minDistance = 2f;
        source.maxDistance = 50f;
        source.rolloffMode = AudioRolloffMode.Logarithmic;

        source.pitch = Random.Range(minPitch, maxPitch);

        source.Play();
        Destroy(audioObj, explosionSound.length + 0.1f);
    }
}