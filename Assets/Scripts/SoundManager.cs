using UnityEngine;

/// <summary>
/// Manages all game sound effects and audio playback
/// </summary>
public class SoundManager : MonoBehaviour
{
    private static SoundManager s_Instance;
    public static SoundManager Instance => s_Instance;

    [Header("Collectible Sounds")]
    [SerializeField] private AudioClip m_GoldPickupSound;
    [SerializeField] private AudioClip m_FuelPickupSound;
    [SerializeField] private AudioClip m_EnergyPickupSound;
    [SerializeField] private AudioClip m_MetalPickupSound;
    
    [Header("Rocket Sounds")]
    [SerializeField] private AudioSource m_ThrusterAudio;  // Reference to thruster AudioSource
    [SerializeField] private float m_ThrusterVolume = 0.7f;
    [SerializeField] private float m_AudioFadeTime = 0.2f;

    private float m_TargetVolume;
    private float m_CurrentVolume;
    private bool m_IsFading;
    
    [Header("Settings")]
    [SerializeField] private float m_DefaultVolume = 0.7f;

    private void Awake()
    {
        if (s_Instance == null)
        {
            s_Instance = this;
            DontDestroyOnLoad(gameObject);

            // Ensure AudioSource is set up
            if (m_ThrusterAudio == null)
            {
                m_ThrusterAudio = gameObject.AddComponent<AudioSource>();
                m_ThrusterAudio.playOnAwake = false;
                m_ThrusterAudio.loop = true;
                m_ThrusterAudio.volume = m_ThrusterVolume;
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        // Handle thruster sound fading
        if (m_IsFading && m_ThrusterAudio != null)
        {
            m_CurrentVolume = Mathf.MoveTowards(m_CurrentVolume, m_TargetVolume, Time.deltaTime / m_AudioFadeTime);
            m_ThrusterAudio.volume = m_CurrentVolume;

            if (Mathf.Approximately(m_CurrentVolume, m_TargetVolume))
            {
                m_IsFading = false;
                if (m_TargetVolume == 0f)
                {
                    m_ThrusterAudio.Stop();
                }
            }
        }
    }

    /// <summary>
    /// Plays the appropriate sound for collecting an item
    /// </summary>
    public void PlayCollectibleSound(CollectibleType type, Vector3 position, float volume = -1f)
    {
        AudioClip soundToPlay = type switch
        {
            CollectibleType.Gold => m_GoldPickupSound,
            CollectibleType.Fuel => m_FuelPickupSound,
            CollectibleType.Energy => m_EnergyPickupSound,
            CollectibleType.Metal => m_MetalPickupSound,
            _ => m_GoldPickupSound
        };

        if (soundToPlay != null)
        {
            float actualVolume = volume < 0 ? m_DefaultVolume : volume;
            AudioSource.PlayClipAtPoint(soundToPlay, position, actualVolume);
        }
    }

    /// <summary>
    /// Controls the rocket's thruster sound
    /// </summary>
    public void SetThrusterSound(bool isThrusting)
    {
        if (m_ThrusterAudio == null) return;

        if (isThrusting)
        {
            if (!m_ThrusterAudio.isPlaying)
            {
                m_ThrusterAudio.Play();
            }
            m_TargetVolume = m_ThrusterVolume;
        }
        else
        {
            m_TargetVolume = 0f;
        }

        m_IsFading = true;
    }

    /// <summary>
    /// Immediately stops all rocket sounds
    /// </summary>
    public void StopRocketSounds()
    {
        if (m_ThrusterAudio != null)
        {
            m_ThrusterAudio.Stop();
            m_IsFading = false;
            m_CurrentVolume = 0f;
        }
    }
} 