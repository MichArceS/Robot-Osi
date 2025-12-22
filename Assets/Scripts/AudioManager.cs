using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    public AudioClip audioClip1;
    public AudioClip audioClip2;
    public AudioClip audioClip3;
    public AudioClip audioClip4;
    public AudioClip audioClip5;
    public AudioClip audioClip6;
    public float fadeDuration = 0.3f;

    private AudioSource audioSource1;
    private AudioSource audioSource2;
    private AudioSource audioSource3;
    private AudioSource audioSource4;
    private AudioSource audioSource5;
    private AudioSource audioSource6;
    private string currentClipName;

    public static AudioManager instance;
    [Range(0f, 0.5f)]
    public float musicVolume = 0.5f;
    [Range(0f, 0.5f)]
    public float sfxVolume = 0.5f;
    [Range(0f, 0.5f)]
    public float voiceVolume = 0.5f;

    private AudioSource musicSource;
    private AudioSource sfxSource;
    private AudioSource voiceSource;

    public Slider musicSlider;
    public Slider sfxSlider;
    public Slider voiceSlider;

    private List<AudioSource> musicSources = new List<AudioSource>();
    private List<AudioSource> sfxSources = new List<AudioSource>();
    private List<AudioSource> voiceSources = new List<AudioSource>();

    public AudioMixer audioMixer; // Drag your Audio Mixer asset here in the Inspector

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            musicSource = gameObject.AddComponent<AudioSource>();
            sfxSource = gameObject.AddComponent<AudioSource>();
            voiceSource = gameObject.AddComponent<AudioSource>();
        }
        else
        {
            Destroy(gameObject);
        }

        musicSlider = GameObject.Find("MusicSlider")?.GetComponent<Slider>();
        sfxSlider = GameObject.Find("SFXSlider")?.GetComponent<Slider>();
        voiceSlider = GameObject.Find("VoiceSlider")?.GetComponent<Slider>();
        AudioSource[] audioSources = GetComponents<AudioSource>();

        if (audioSources.Length < 6)
        {
            return;
        }

        audioSource1 = audioSources[0];
        audioSource2 = audioSources[1];
        audioSource3 = audioSources[2];
        audioSource4 = audioSources[3];
        audioSource5 = audioSources[4];
        audioSource6 = audioSources[5];

        audioSource1.loop = true;
        audioSource2.loop = true;
        audioSource3.loop = true;
        audioSource4.loop = true;
        audioSource5.loop = true;
        audioSource6.loop = true;

        audioSource1.volume = 0f;
        audioSource2.volume = 0f;
        audioSource3.volume = 0f;
        audioSource4.volume = 0f;
        audioSource5.volume = 0f;
        audioSource6.volume = 0f;

        audioSource1.clip = audioClip1;
        audioSource2.clip = audioClip2;
        audioSource3.clip = audioClip3;
        audioSource4.clip = audioClip4;
        audioSource5.clip = audioClip5;
        audioSource6.clip = audioClip6;

        audioSource1.Play();
        StartCoroutine(FadeInAudio(audioSource1, musicVolume));
        currentClipName = "audioClip1";
    }

    private void Start()
    {

        musicSlider = GameObject.Find("MusicSlider")?.GetComponent<Slider>();
        sfxSlider = GameObject.Find("SFXSlider")?.GetComponent<Slider>();
        voiceSlider = GameObject.Find("VoiceSlider")?.GetComponent<Slider>();

        if (musicSlider != null)
        {
            musicSlider.value = musicVolume;
        }
        if (sfxSlider != null)
        {
            sfxSlider.value = sfxVolume;
        }
        if (voiceSlider != null)
        {
            voiceSlider.value = voiceVolume;
        }

        RegisterMusicSource(audioSource1);
        RegisterMusicSource(audioSource2);
        RegisterMusicSource(audioSource3);
        RegisterMusicSource(audioSource4);
        RegisterMusicSource(audioSource5);
        RegisterMusicSource(audioSource6);
        // Register all audio sources in the scene
    }

    private void OnEnable()
    {
        // Register all audio sources in the current scene
        RegisterAllAudioSources();

        // Subscribe to the scene change event
        SceneManager.activeSceneChanged += OnSceneChanged;
    }

    private void OnDisable()
    {
        // Unsubscribe from the scene change event
        SceneManager.activeSceneChanged -= OnSceneChanged;
    }

    private void OnSceneChanged(Scene prevScene, Scene newScene)
    {
        // Register all audio sources in the new scene
        RegisterAllAudioSources();

        // Dynamically find sliders in the new scene
        musicSlider = GameObject.Find("MusicSlider")?.GetComponent<Slider>();
        sfxSlider = GameObject.Find("SFXSlider")?.GetComponent<Slider>();
        voiceSlider = GameObject.Find("VoiceSlider")?.GetComponent<Slider>();

        // If sliders are found, attach listeners for updates
        if (musicSlider != null)
        {
            musicSlider.onValueChanged.AddListener(SetMusicVolume);
            musicSlider.value = musicVolume;
        }
        if (sfxSlider != null)
        {
            sfxSlider.onValueChanged.AddListener(SetSFXVolume);
            sfxSlider.value = sfxVolume;
        }
        if (voiceSlider != null)
        {
            voiceSlider.onValueChanged.AddListener(SetVoiceVolume);
            voiceSlider.value = voiceVolume;
        }
    }

    private void RegisterAllAudioSources()
    {
        // Find all AudioSource components in the current scene
        AudioSource[] allAudioSources = FindObjectsOfType<AudioSource>(true);

        // Register each audio source with the appropriate list in the AudioManager
        foreach (var source in allAudioSources)
        {
            if (source.gameObject.scene == gameObject.scene)
            {
                // Check if the AudioSource has an OutputAudioMixerGroup assigned
                if (source.outputAudioMixerGroup == null)
                {
                    // Try to assign the default "SFX" group
                    AudioMixerGroup[] sfxGroups = audioMixer.FindMatchingGroups("SFX");

                    if (sfxGroups.Length > 0) // Ensure "SFX" group exists
                    {
                        source.outputAudioMixerGroup = sfxGroups[0];
                    }
                    else
                    {
                        Debug.LogWarning($"No 'SFX' AudioMixerGroup found for AudioSource on GameObject '{source.gameObject.name}'");
                    }
                }
                if (source.outputAudioMixerGroup?.name == "Music")
                {
                    RegisterMusicSource(source);
                }
                else if (source.outputAudioMixerGroup?.name == "SFX")
                {
                    RegisterSFXSource(source);
                }
                else if (source.outputAudioMixerGroup?.name == "Voice")
                {
                    RegisterVoiceSource(source);
                }
                else
                {
                    // Handle unrecognized groups (optional)
                    Debug.Log($"AudioSource on '{source.gameObject.name}' has an unrecognized group: {source.outputAudioMixerGroup?.name}");
                }
            }
        }
    }

    private IEnumerator FadeInAudio(AudioSource audioSource, float targetVolume)
    {
        float startVolume = 0f;
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, targetVolume, elapsedTime / fadeDuration);
            yield return null;
        }
        audioSource.volume = targetVolume;
    }

    public void SwitchAudio(string clipName)
    {
        if (currentClipName == clipName)
        {
            return;
        }

        StartCoroutine(FadeAudio(clipName));
    }

    private IEnumerator FadeAudio(string clipName)
    {
        AudioSource currentAudio = GetAudioSourceByName(currentClipName);
        AudioSource nextAudio = GetAudioSourceByName(clipName);

        // Fade out
        float currentVolume = currentAudio.volume;
        float targetVolume = 0f;
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            currentAudio.volume = Mathf.Lerp(currentVolume, targetVolume, elapsedTime / fadeDuration);
            yield return null;
        }
        currentAudio.volume = targetVolume;
        currentAudio.Stop();

        // Prepara el nuevo audio
        nextAudio.volume = 0.001f;
        nextAudio.Play();

        // Fade in
        elapsedTime = 0f;
        targetVolume = 0.1f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            nextAudio.volume = Mathf.Lerp(0f, targetVolume, elapsedTime / fadeDuration);
            yield return null;
        }
        nextAudio.volume = targetVolume;

        currentClipName = clipName;
    }

    private AudioSource GetAudioSourceByName(string clipName)
    {
        switch (clipName)
        {
            case "audioClip1": return audioSource1;
            case "audioClip2": return audioSource2;
            case "audioClip3": return audioSource3;
            case "audioClip4": return audioSource4;
            case "audioClip5": return audioSource5;
            case "audioClip6": return audioSource6;
            default: return null;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Lista de escenas permitidas para la reproducci�n de audio
        string[] allowedScenes = {
            "Pantalla_Inicio",
            "Pantalla_Nuevo_Juego",
            "Pantalla_Avatars",
            "Pantalla_Nombre",
            "Pantalla_eleccion_avatars_creates",
            "Pantalla_Actividades",
            "Pantalla_select_clima",
            "Pantalla_select_fenomenos",
            "Pantalla_select_lugares",
            "historia_clima",
            "juegos_clima",
            "historia_fenomenos_naturales",
            "juegos_fenomenos_naturales",
            "historia_lugares_publicos",
            "terremoto_juego",
            "terremoto_juego2",
            "inundacion_juego",
            "inundacion_juego2",
            "erupcion_volcanica",
            "erupcion_Volcanica2",
            "recompensas_juego2",
            "instruccion_juego1",
            "recompensa1_jego2",
            "recompensa3_juego2"

        };

        if (System.Array.Exists(allowedScenes, s => s == scene.name))
        {
            switch (scene.name)
            {
                case "Pantalla_Inicio":
                case "Pantalla_Nuevo_Juego":
                    SwitchAudio("audioClip1");
                    break;
                case "Pantalla_Avatars":
                case "Pantalla_Nombre":
                case "Pantalla_eleccion_avatars_creates":
                case "Pantalla_Actividades":
                case "Pantalla_select_clima":
                case "Pantalla_select_fenomenos":
                case "Pantalla_select_lugares":
                case "juegos_fenomenos_naturales":
                case "instruccion_juego1":
                    SwitchAudio("audioClip2");
                    break;
                case "historia_fenomenos_naturales":
                case "juegos_clima":
                    SwitchAudio("audioClip3");
                    break;
                case "terremoto_juego":
                case "terremoto_juego2":
                case "inundacion_juego":
                case "inundacion_juego2":
                case "erupcion_volcanica":
                case "erupcion_Volcanica2":
                case "recompensas_juego2":
                case "recompensa1_jego2":
                case "recompensa3_jego2":
                    SwitchAudio("audioClip4");
                    break;
                case "historia_clima":
                    SwitchAudio("audioClip5");
                    break;
                case "historia_lugares_publicos":
                    SwitchAudio("audioClip6");
                    break;
                default:
                    SwitchAudio("audioClip2");
                    break;
            }
        }
        else
        {
            audioSource1.Stop();
            audioSource2.Stop();
            audioSource3.Stop();
            audioSource4.Stop();
        }
    }

    public void SetMusicVolume(float volume)
    {
        // Clamp the slider value between 0 (silent) and 0.5f (50% volume)
        musicVolume = Mathf.Clamp(volume, 0f, 0.5f);

        // Convert volume to decibels
        // Use `0.5f` as the maximum volume (50% = 0 dB, 0% = -80 dB)
        float normalizedVolume = musicVolume / 0.5f; // Normalize to 0 - 1 range
        float dB = (normalizedVolume > 0) ? Mathf.Log10(normalizedVolume) * 20 : -80f;

        // Update the Audio Mixer's exposed parameter
        audioMixer.SetFloat("MusicVolume", dB);

        // Update the slider, if present
        if (musicSlider != null)
        {
            musicSlider.value = musicVolume;
        }
    }

    public void SetSFXVolume(float volume)
    {
        // Clamp the slider value between 0 and 0.5f (50% volume cap)
        sfxVolume = Mathf.Clamp(volume, 0f, 0.5f);

        // Convert volume to decibels (normalized to 0-1 for Audio Mixer)
        float normalizedVolume = sfxVolume / 0.5f; // Normalize to 0 - 1 range
        float dB = (normalizedVolume > 0) ? Mathf.Log10(normalizedVolume) * 20 : -80f;

        // Update the Audio Mixer's SFX volume parameter
        audioMixer.SetFloat("SFXVolume", dB);

        // Update the slider, if present
        if (sfxSlider != null)
        {
            sfxSlider.value = sfxVolume;
        }
    }

    public void SetVoiceVolume(float volume)
    {
        // Clamp the slider value between 0 and 0.5f (50% volume cap)
        voiceVolume = Mathf.Clamp(volume, 0f, 0.5f);

        // Convert volume to decibels (normalized to 0-1 for Audio Mixer)
        float normalizedVolume = voiceVolume / 0.5f; // Normalize to 0 - 1 range
        float dB = (normalizedVolume > 0) ? Mathf.Log10(normalizedVolume) * 20 : -80f;

        // Update the Audio Mixer's Voice volume parameter
        audioMixer.SetFloat("VoiceVolume", dB);

        // Update the slider, if present
        if (voiceSlider != null)
        {
            voiceSlider.value = voiceVolume;
        }
    }

    public void PlayMusic(AudioClip clip)
    {
        musicSource.clip = clip;
        musicSource.Play();
    }

    public void PlaySFX(AudioClip clip)
    {
        sfxSource.PlayOneShot(clip);
    }

    public void PlayVoice(AudioClip clip)
    {
        voiceSource.PlayOneShot(clip);
    }

    public void RegisterMusicSource(AudioSource source)
    {
        if (!musicSources.Contains(source))
        {
            musicSources.Add(source);
            source.volume = musicVolume;
        }
    }

    public void RegisterSFXSource(AudioSource source)
    {
        if (!sfxSources.Contains(source))
        {
            sfxSources.Add(source);
            source.volume = sfxVolume;
        }
    }

    public void RegisterVoiceSource(AudioSource source)
    {
        if (!voiceSources.Contains(source))
        {
            voiceSources.Add(source);
            source.volume = voiceVolume;
        }
    }

    public void UnregisterMusicSource(AudioSource source)
    {
        musicSources.Remove(source);
    }

    public void UnregisterSFXSource(AudioSource source)
    {
        sfxSources.Remove(source);
    }

    public void UnregisterVoiceSource(AudioSource source)
    {
        voiceSources.Remove(source);
    }
}
