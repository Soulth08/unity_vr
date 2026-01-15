using UnityEngine;
using TMPro;

public class BombCountdown : MonoBehaviour
{
    [Header("Param�tres Temps")]
    public float tempsAvantExplosion = 20f;
    public float vitesseClignotementStart = 0.1f;
    public float vitesseClignotementEnd = 2f;

    [Header("Feedback Visuel")]
    public Renderer bombRenderer;
    [ColorUsage(true, true)]
    public Color couleurFlash = Color.white;
    public GameObject bombDestroyEffect;

    [Header("Explosion")]
    public AudioClip explosionSound;
    [Range(0f, 1f)] public float volumeExplosion = 1f;

    // comme spatialBlend, permet de choisir si le son est plus r�parti entre les deux canaux uniform�ment ou ind�pendemment selon la direction
    // c'est mieux de mettre des valeurs 3D, car cela permet d'identifier rapidement o� se situe la bombe
    [Tooltip("0 = 2D, 1 = 3D")] 
    [Range(0f, 1f)] public float spatialBlend = 1f;
    public float minPitch = 0.8f;
    public float maxPitch = 1.2f;

    [Header("Compte � rebours")]
    [Tooltip("Le son du compte � rebours. La fin du fichier sera synchronis�e avec l'explosion.")]
    public AudioClip timerSound;
    [Range(0f, 1f)] public float volumeTimer = 1f;

    [Header("UI")]
    public TMP_Text timerText; // pas utilis� ici, mais permet d'afficher le temps restant au dessus de la bombe

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

        // SETUP AUDIO Compte � rebours
        if (timerSound != null)
        {
            // On ajoute un AudioSource directement sur la bombe pour qu'il la suive
            timerAudioSource = gameObject.AddComponent<AudioSource>();
            timerAudioSource.clip = timerSound;
            timerAudioSource.volume = volumeTimer;
            timerAudioSource.spatialBlend = spatialBlend;
            timerAudioSource.loop = false; // Important : pas de boucle
            timerAudioSource.playOnAwake = false;

            
            // Si le son est plus long que le timer (ex: Son 30s, Timer 20s)
            // On doit jouer imm�diatement, mais en sautant le d�but pour ne jouer que la fin
            if (timerSound.length >= tempsAvantExplosion)
            {
                timerAudioSource.time = timerSound.length - tempsAvantExplosion;
                timerAudioSource.Play();
                timerSoundStarted = true;
            }
            // Si le son est plus court (ex: Son 5s, Timer 20s)
            // On ne fait rien ici, on attendra dans l'Update
        }
    }

    private void Update()
    {
        if (GameManager.Instance.isGameOver || aExplose) return;

        timerActuel -= Time.deltaTime;

        // gestion du son dans le cas o� le fichier est plus court
        // il faut le synchroniser pour qu'il se termine exactement au moment de l'explosion
        if (timerAudioSource != null && !timerSoundStarted)
        {
            // Si le temps restant est inf�rieur ou �gal � la dur�e du son, on lance !
            if (timerActuel <= timerSound.length)
            {
                timerAudioSource.Play();
                timerSoundStarted = true;
            }
        }


        if (timerText != null)
        {
            timerText.text = timerActuel.ToString("F0");
            if (timerActuel <= 5) timerText.color = Color.red;
        }

        if (bombRenderer != null) ApplyBlinkingEffect();

        if (timerActuel <= 0) Explode(); // Explosioooooooooooon
    }

    private void ApplyBlinkingEffect()
    {
        float progression = 1 - (timerActuel / tempsAvantExplosion); // valeur entre 0 (d�but) et 1 (fin) indiquant l'avanc�e du compte � rebours
        float vitesseActuelle = Mathf.Lerp(vitesseClignotementStart, vitesseClignotementEnd, progression); // fait une conversion de la progession en vitesse de clignotement
        float lerp = (Mathf.Sin(Time.time * vitesseActuelle) + 1f) / 2f; // on utilise sin pour avoir un blinking plus doux que le ON/OFF utilis� pour le texte de l'ordinateur

        Color finalEmission = Color.Lerp(Color.black, couleurFlash, lerp); // le noir est la couleur invisible pour l'�mission
        bombRenderer.material.SetColor(emissionColorId, finalEmission);
    }

    private void Explode()
    {
        aExplose = true;
        Debug.Log("Explosiooooooooon !");

        if (GameManager.Instance != null) GameManager.Instance.TakeDamage(1, true);

        if (bombDestroyEffect != null)
            Instantiate(bombDestroyEffect, transform.position, transform.rotation);

        if (explosionSound != null)
        {
            PlayCustomSound();
        }

        // En d�truisant l'objet, l'AudioSource du timer attach� dessus sera coup� net
        // le tic-tac s'arr�te quand �a explose
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