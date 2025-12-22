using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalCounterInitializer : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioSource audioSourceIncorrect;

    public AudioClip[] audioClipsForThirdCorrectAnswer;
    public AudioClip[] audioClipsForThirdIncorrectAnswer;

    void Start()
    {
        if (audioSource != null)
        {
            GlobalCounter.InitializeAudioSource(audioSource);
        }
        else
        {
            Debug.LogWarning("No se ha asignado el AudioSource en GlobalCounterInitializer.");
        }

        if (audioSourceIncorrect != null)
        {
            GlobalCounter.InitializeAudioSourceIncorrect(audioSourceIncorrect);
        }
        else
        {
            Debug.LogWarning("No se ha asignado el AudioSource en GlobalCounterInitializer.");
        }

        if (audioClipsForThirdCorrectAnswer != null && audioClipsForThirdCorrectAnswer.Length > 0)
        {
            GlobalCounter.SetAudioClips(audioClipsForThirdCorrectAnswer);
        }
        else
        {
            Debug.LogWarning("No se han asignado AudioClips en GlobalCounterInitializer.");
        }

        if (audioClipsForThirdIncorrectAnswer != null && audioClipsForThirdIncorrectAnswer.Length > 0)
        {
            GlobalCounter.SetAudioClipsIncorrect(audioClipsForThirdIncorrectAnswer);
        }
        else
        {
            Debug.LogWarning("No se han asignado AudioClips en GlobalCounterInitializer.");
        }
    }
}
