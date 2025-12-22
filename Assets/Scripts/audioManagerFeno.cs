using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class audioManagerFeno : MonoBehaviour
{
    public AudioSource audioSource; // Referencia al componente AudioSource
    public AudioClip[] audioClips; // Array para almacenar los clips de audio
    public Button[] buttons; // Array para almacenar los botones

    void Start()
    {
        // Asigna cada botón a su correspondiente clip de audio
        for (int i = 0; i < buttons.Length; i++)
        {
            int index = i; // Necesario para evitar el cierre de la variable en el loop
            buttons[i].onClick.AddListener(() => PlayAudio(index));
        }
    }

    void PlayAudio(int index)
    {
        if (audioSource != null && audioClips.Length > index)
        {
            audioSource.clip = audioClips[index]; // Asigna el clip de audio
            audioSource.Play(); // Reproduce el clip de audio
        }
    }
}
