using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class WelcomeScreenButton : MonoBehaviour
{
    public string pantallaAvatar;
    public string pantallaNuevoJuego;
    public AudioSource audioSource;

    void Start()
    {
        if (audioSource == null)
        {
            audioSource = FindObjectOfType<AudioSource>();
        }

        Button button = GetComponent<Button>();
        button.onClick.AddListener(OnButtonClick);
    }

    public void OnButtonClick()
    {
        // Verificar si es la primera vez que juega
        string sceneToLoad;

        if (PlayerPrefs.GetInt("FirstTime", 1) == 1 && !(PlayerPrefs.HasKey("SelectedAvatarImageName") && PlayerPrefs.HasKey("playerName")))
        {
            Debug.Log("Es la primera vez que juega.");
            sceneToLoad = pantallaAvatar;
        }
        else
        {
            Debug.Log("No es la primera vez que juega.");
            sceneToLoad = pantallaNuevoJuego;
        }

        StartCoroutine(PlayAudioAndLoadScene(sceneToLoad));
    }

    IEnumerator PlayAudioAndLoadScene(string sceneToLoad)
    {
        audioSource.Play();
        yield return new WaitForSeconds(audioSource.clip.length);
        SceneManager.LoadScene(sceneToLoad);
    }
}