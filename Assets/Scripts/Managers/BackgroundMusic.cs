using UnityEngine;

public class BackgroundMusic : MonoBehaviour
{
    [Header("Music Settings")]
    [SerializeField] private AudioClip backgroundMusic;
    [SerializeField] private bool playOnStart = true;
    [SerializeField] private bool loopMusic = true;
    
    private void Start()
    {
        if (playOnStart && backgroundMusic != null)
        {
            PlayBackgroundMusic();
        }
    }
    
    public void PlayBackgroundMusic()
    {
        if (AudioManager.Instance != null && backgroundMusic != null)
        {
            AudioManager.Instance.PlayMusic(backgroundMusic, loopMusic);
        }
    }
    
    public void StopBackgroundMusic()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopMusic();
        }
    }
    
    public void PauseBackgroundMusic()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PauseMusic();
        }
    }
    
    public void ResumeBackgroundMusic()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.ResumeMusic();
        }
    }
}
