using UnityEngine;
using UnityEngine.UI;

public class AudioVolumeUIManager : MonoBehaviour
{
    public Slider musicSlider;
    public Slider sfxSlider;
    public Slider voiceSlider;

    private void Start()
    {
        if (musicSlider == null)
        {
            musicSlider = GameObject.Find("MusicSlider").GetComponent<Slider>();
        }
        if (sfxSlider == null)
        {
            sfxSlider = GameObject.Find("SFXSlider").GetComponent<Slider>();
        }
        if (voiceSlider == null)
        {
            voiceSlider = GameObject.Find("VoiceSlider").GetComponent<Slider>();
        }
        if (musicSlider == null || sfxSlider == null || voiceSlider == null)
        {
            Debug.LogError("One or more sliders are not assigned in the inspector.");
            return;
        }
        musicSlider.maxValue = 0.5f;
        sfxSlider.maxValue = 0.5f;
        voiceSlider.maxValue = 0.5f;

        if (AudioManager.instance == null)
        {
            Debug.LogError("AudioManager instance is not assigned.");
            return;
        }

        // Link the Sliders to the AudioManager
        musicSlider.value = AudioManager.instance.musicVolume;
        sfxSlider.value = AudioManager.instance.sfxVolume;
        voiceSlider.value = AudioManager.instance.voiceVolume;

        // Add event listeners to update the AudioManager when Slider values change
        musicSlider.onValueChanged.AddListener(AudioManager.instance.SetMusicVolume);
        sfxSlider.onValueChanged.AddListener(AudioManager.instance.SetSFXVolume);
        voiceSlider.onValueChanged.AddListener(AudioManager.instance.SetVoiceVolume);
    }
}