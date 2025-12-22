using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExitGame : MonoBehaviour
{
    public Button exitButton;
    public Button showConfirmationButton;
    public Button cancelButton;
    public AudioSource audioSource;
    public GameObject confirmationPanel;

    void Start()
    {
        if (exitButton != null)
        {
            exitButton.onClick.AddListener(OnExitButtonClicked);
        }
        if (showConfirmationButton != null)
        {
            showConfirmationButton.onClick.AddListener(ShowConfirmationPanel);
        }
        if (cancelButton != null)
        {
            cancelButton.onClick.AddListener(HideConfirmationPanel);
        }
    }

    void ShowConfirmationPanel()
    {
        if (confirmationPanel != null)
        {
            confirmationPanel.SetActive(true);
        }
    }

    public void HideConfirmationPanel()
    {
        if (confirmationPanel != null)
        {
            confirmationPanel.SetActive(false);
        }
    }

    void OnExitButtonClicked()
    {
        // Reproduce el sonido
        if (audioSource != null)
        {
            audioSource.Play();
            // Espera a que termine el audio para salir
            Invoke("QuitGame", audioSource.clip.length);
        }
    }

    void QuitGame()
    {
        // Para editor
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
    }
}
