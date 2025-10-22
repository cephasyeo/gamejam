using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Helper script to easily connect UI sliders to OptionsMenu functions
/// Attach this to volume sliders for quick setup
/// </summary>
public class VolumeSliderHelper : MonoBehaviour
{
    [Header("Slider Type")]
    [SerializeField] private VolumeType volumeType;
    
    private enum VolumeType
    {
        Master,
        Music,
        SFX
    }
    
    private void Start()
    {
        Slider slider = GetComponent<Slider>();
        OptionsMenu optionsMenu = FindFirstObjectByType<OptionsMenu>();
        
        if (slider != null && optionsMenu != null)
        {
            switch (volumeType)
            {
                case VolumeType.Master:
                    slider.onValueChanged.AddListener(optionsMenu.OnMasterVolumeChanged);
                    break;
                case VolumeType.Music:
                    slider.onValueChanged.AddListener(optionsMenu.OnMusicVolumeChanged);
                    break;
                case VolumeType.SFX:
                    slider.onValueChanged.AddListener(optionsMenu.OnSFXVolumeChanged);
                    break;
            }
        }
    }
}
