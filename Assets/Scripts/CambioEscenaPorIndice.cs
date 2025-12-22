using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CambioEscenaPorIndice : MonoBehaviour
{
    public int indiceEscena = 0;
    public AudioSource audioSource; 

    void Start()
    {
        if (audioSource == null)
        {
            audioSource = FindObjectOfType<AudioSource>();
        }
    }

    public void CambiarEscenaPorIndice()
    {
        StartCoroutine(PlayAudioAndLoadScene());
    }

    IEnumerator PlayAudioAndLoadScene()
    {
        audioSource.Play(); 
        yield return new WaitForSeconds(audioSource.clip.length);
        SceneManager.LoadSceneAsync(indiceEscena);
    }
}
