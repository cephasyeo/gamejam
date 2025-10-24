using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer audioMixer;
    
    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;
    
    [Header("Game SFX")]
    [SerializeField] private AudioClip jumpSFX;
    [SerializeField] private AudioClip orbCollectSFX;
    [SerializeField] private AudioClip dashSFX;
    
    [Header("Default Volume Levels")]
    [SerializeField] private float defaultMasterVolume = 1f;
    [SerializeField] private float defaultMusicVolume = 0.8f;
    [SerializeField] private float defaultSFXVolume = 1f;
    
    [Header("Debug")]
    [SerializeField] private bool debugMode = true;
    
    // Singleton instance
    public static AudioManager Instance { get; private set; }
    
    // Volume properties
    public float MasterVolume { get; private set; }
    public float MusicVolume { get; private set; }
    public float SFXVolume { get; private set; }
    
    // Events
    public System.Action<float> OnMasterVolumeChanged;
    public System.Action<float> OnMusicVolumeChanged;
    public System.Action<float> OnSFXVolumeChanged;
    
    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudio();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void InitializeAudio()
    {
        // Load saved volume settings or use defaults
        MasterVolume = PlayerPrefs.GetFloat("MasterVolume", defaultMasterVolume);
        MusicVolume = PlayerPrefs.GetFloat("MusicVolume", defaultMusicVolume);
        SFXVolume = PlayerPrefs.GetFloat("SFXVolume", defaultSFXVolume);
        
        // Apply volume settings
        SetMasterVolume(MasterVolume);
        SetMusicVolume(MusicVolume);
        SetSFXVolume(SFXVolume);
        
        if (debugMode)
        {
            Debug.Log("AudioManager initialized");
        }
    }
    
    #region Volume Control Methods
    
    public void SetMasterVolume(float volume)
    {
        MasterVolume = Mathf.Clamp01(volume);
        
        // Convert linear volume to decibels for AudioMixer
        float dbVolume = MasterVolume > 0 ? Mathf.Log10(MasterVolume) * 20 : -80f;
        
        if (audioMixer != null)
        {
            audioMixer.SetFloat("MasterVolume", dbVolume);
        }
        
        // Save to PlayerPrefs
        PlayerPrefs.SetFloat("MasterVolume", MasterVolume);
        PlayerPrefs.Save();
        
        OnMasterVolumeChanged?.Invoke(MasterVolume);
        
        if (debugMode)
        {
            Debug.Log($"Master volume set to: {MasterVolume:F2}");
        }
    }
    
    public void SetMusicVolume(float volume)
    {
        MusicVolume = Mathf.Clamp01(volume);
        
        // Convert linear volume to decibels for AudioMixer
        float dbVolume = MusicVolume > 0 ? Mathf.Log10(MusicVolume) * 20 : -80f;
        
        if (audioMixer != null)
        {
            audioMixer.SetFloat("MusicVolume", dbVolume);
        }
        
        // Save to PlayerPrefs
        PlayerPrefs.SetFloat("MusicVolume", MusicVolume);
        PlayerPrefs.Save();
        
        OnMusicVolumeChanged?.Invoke(MusicVolume);
        
        if (debugMode)
        {
            Debug.Log($"Music volume set to: {MusicVolume:F2}");
        }
    }
    
    public void SetSFXVolume(float volume)
    {
        SFXVolume = Mathf.Clamp01(volume);
        
        // Convert linear volume to decibels for AudioMixer
        float dbVolume = SFXVolume > 0 ? Mathf.Log10(SFXVolume) * 20 : -80f;
        
        if (audioMixer != null)
        {
            audioMixer.SetFloat("SFXVolume", dbVolume);
        }
        
        // Save to PlayerPrefs
        PlayerPrefs.SetFloat("SFXVolume", SFXVolume);
        PlayerPrefs.Save();
        
        OnSFXVolumeChanged?.Invoke(SFXVolume);
        
        if (debugMode)
        {
            Debug.Log($"SFX volume set to: {SFXVolume:F2}");
        }
    }
    
    #endregion
    
    #region Audio Playback Methods
    
    public void PlayMusic(AudioClip clip, bool loop = true)
    {
        if (musicSource != null && clip != null)
        {
            // Don't restart if the same music is already playing
            if (musicSource.isPlaying && musicSource.clip == clip)
            {
                if (debugMode)
                {
                    Debug.Log($"Music already playing: {clip.name}");
                }
                return;
            }
            
            musicSource.clip = clip;
            musicSource.loop = loop;
            musicSource.Play();
            
            if (debugMode)
            {
                Debug.Log($"Playing music: {clip.name}");
            }
        }
    }
    
    public void StopMusic()
    {
        if (musicSource != null)
        {
            musicSource.Stop();
            
            if (debugMode)
            {
                Debug.Log("Music stopped");
            }
        }
    }
    
    public void PauseMusic()
    {
        if (musicSource != null)
        {
            musicSource.Pause();
            
            if (debugMode)
            {
                Debug.Log("Music paused");
            }
        }
    }
    
    public void ResumeMusic()
    {
        if (musicSource != null)
        {
            musicSource.UnPause();
            
            if (debugMode)
            {
                Debug.Log("Music resumed");
            }
        }
    }
    
    public void PlaySFX(AudioClip clip, float volumeMultiplier = 1f)
    {
        if (sfxSource != null && clip != null)
        {
            sfxSource.PlayOneShot(clip, volumeMultiplier);
            
            if (debugMode)
            {
                Debug.Log($"Playing SFX: {clip.name}");
            }
        }
    }
    
    public void PlaySFXAtPosition(AudioClip clip, Vector3 position, float volumeMultiplier = 1f)
    {
        if (clip != null)
        {
            AudioSource.PlayClipAtPoint(clip, position, volumeMultiplier * SFXVolume);
            
            if (debugMode)
            {
                Debug.Log($"Playing SFX at position: {clip.name}");
            }
        }
    }
    
    // Game-specific SFX methods
    public void PlayJumpSFX()
    {
        if (jumpSFX != null)
        {
            PlaySFX(jumpSFX);
        }
    }
    
    public void PlayOrbCollectSFX()
    {
        if (orbCollectSFX != null)
        {
            PlaySFX(orbCollectSFX);
        }
    }
    
    public void PlayDashSFX()
    {
        if (dashSFX != null)
        {
            PlaySFX(dashSFX);
        }
    }
    
    #endregion
    
    #region Utility Methods
    
    public void ResetToDefaults()
    {
        SetMasterVolume(defaultMasterVolume);
        SetMusicVolume(defaultMusicVolume);
        SetSFXVolume(defaultSFXVolume);
        
        if (debugMode)
        {
            Debug.Log("Audio settings reset to defaults");
        }
    }
    
    public void MuteAll()
    {
        SetMasterVolume(0f);
        
        if (debugMode)
        {
            Debug.Log("All audio muted");
        }
    }
    
    public void UnmuteAll()
    {
        SetMasterVolume(defaultMasterVolume);
        
        if (debugMode)
        {
            Debug.Log("All audio unmuted");
        }
    }
    
    public bool IsMusicPlaying()
    {
        return musicSource != null && musicSource.isPlaying;
    }
    
    public bool IsMusicPaused()
    {
        return musicSource != null && !musicSource.isPlaying && musicSource.time > 0;
    }
    
    #endregion
}
