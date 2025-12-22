using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PanelManagerTransition : MonoBehaviour
{
    public GameObject panel2;
    public GameObject panel3;
    public GameObject panel4;
    public AudioSource audioButton1;
    public AudioSource audioButton2;
    public AudioSource audioButton3;
    public AudioSource audioInstrucion;

    public Animator animator1;
    public Animator animator2;
    public Animator animator3;

    public string[] scenes;
    public float waitTimeBeforePanel2Transition = 1f;
    public float waitTimeBeforePanel3Transition = 0.5f;
    public float transitionDuration = 1f;

    private AudioSource activeAudioSource;
    private bool isInInstructionPanel = false;
    private bool isSkipping = false;
    private int currentSceneIndex = 0;
    private Coroutine currentTransitionCoroutine;

    private void Start()
    {
        panel2.SetActive(false);
        panel3.SetActive(false);
        panel4.SetActive(false);
    }

    private void Update()
    {
        if (isInInstructionPanel && Input.GetMouseButtonDown(0) && !isSkipping)
        {
            SkipToNextScene();
        }
    }

    private void SkipToNextScene()
    {
        isSkipping = true;

        // Detener el audio actual
        if (activeAudioSource != null && activeAudioSource.isPlaying)
        {
            activeAudioSource.Stop();
        }
        if (audioInstrucion != null && audioInstrucion.isPlaying)
        {
            audioInstrucion.Stop();
        }

        // Detener la corrutina actual si existe
        if (currentTransitionCoroutine != null)
        {
            StopCoroutine(currentTransitionCoroutine);
        }

        // Detener todas las animaciones
        if (animator1 != null) animator1.enabled = false;
        if (animator2 != null) animator2.enabled = false;
        if (animator3 != null) animator3.enabled = false;

        // Cargar la siguiente escena inmediatamente
        SceneManager.LoadScene(scenes[currentSceneIndex]);
    }

    private Coroutine buttonClickCoroutine; // Track the current coroutine


    public void OnButtonClick(int buttonIndex)
    {
        if (buttonIndex < 1 || buttonIndex > scenes.Length)
        {
            Debug.LogError("Botón inválido");
            return;
        }
        // If a coroutine is already running, stop it
        if (buttonClickCoroutine != null)
        {
            StopCoroutine(buttonClickCoroutine);
        }

        currentSceneIndex = buttonIndex - 1;
        buttonClickCoroutine = StartCoroutine(HandleButtonClick(buttonIndex));
    }

    private IEnumerator HandleButtonClick(int buttonIndex)
    {
        yield return new WaitForSeconds(1.5f);

        // Establece el audio activo
        switch (buttonIndex)
        {
            case 1:
                activeAudioSource = audioButton1;
                break;
            case 2:
                activeAudioSource = audioButton2;
                break;
            case 3:
                activeAudioSource = audioButton3;
                break;
        }

        // Reproduce el audio
        if (activeAudioSource != null)
        {
            activeAudioSource.Play();
        }

        currentTransitionCoroutine = StartCoroutine(HandlePanelTransition(buttonIndex - 1));
    }

    private IEnumerator HandlePanelTransition(int sceneIndex)
    {
        panel2.SetActive(true);
        panel3.SetActive(true);

        CanvasGroup canvasGroup2 = panel2.GetComponent<CanvasGroup>();
        CanvasGroup canvasGroup3 = panel3.GetComponent<CanvasGroup>();
        CanvasGroup canvasGroup4 = panel4.GetComponent<CanvasGroup>();

        SetCanvasGroupAlpha(canvasGroup2, 1f);
        SetCanvasGroupAlpha(canvasGroup3, 0f);
        SetCanvasGroupAlpha(canvasGroup4, 0f);

        yield return new WaitForSeconds(waitTimeBeforePanel2Transition);
        Debug.Log("Esperando transici�n de Panel 2 a Panel 3.");
        yield return StartCoroutine(FadeOutAndIn(canvasGroup2, canvasGroup3, transitionDuration, 1f));
        Debug.Log("Transici�n de Panel 2 a Panel 3 completada.");

        yield return new WaitForSeconds(waitTimeBeforePanel3Transition);
        Debug.Log("Esperando transici�n de Panel 3 a Panel 4.");
        panel4.SetActive(true);
        yield return StartCoroutine(FadeOutAndIn(canvasGroup3, canvasGroup4, transitionDuration, 1f));
        Debug.Log("Transici�n de Panel 3 a Panel 4 completada.");



        SetCanvasGroupAlpha(canvasGroup4, 1f);
        isInInstructionPanel = true; // Indicamos que estamos en el panel de instrucciones

        // Reproduce el audio
        if (audioInstrucion != null && !isSkipping)
        {
            activeAudioSource = audioInstrucion;
            audioInstrucion.Play();
        }
        if (!isSkipping)
        {
            yield return StartCoroutine(PlayAnimations());
        }

        if (!isSkipping)
        {
            yield return new WaitForSeconds(2f);
            SceneManager.LoadScene(scenes[sceneIndex]);
        }
    }

    private IEnumerator PlayAnimations()
    {
        animator1.SetTrigger("PlayAnimation1");
        yield return new WaitForSeconds(0.5f);

        animator2.gameObject.SetActive(true);
        yield return new WaitForSeconds(0.3f);
        animator2.gameObject.SetActive(false);

        animator3.SetTrigger("PlayAnimation3");
        yield return new WaitForSeconds(2.5f);
    }

    private IEnumerator FadeOutAndIn(CanvasGroup fadeOutGroup, CanvasGroup fadeInGroup, float duration, float initialFadeInAlpha)
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            fadeOutGroup.alpha = Mathf.Clamp01(1 - (elapsedTime / duration));
            fadeInGroup.alpha = Mathf.Clamp01(Mathf.Lerp(initialFadeInAlpha, 1f, elapsedTime / duration));
            yield return null;
        }

        fadeOutGroup.alpha = 0f;
        fadeInGroup.alpha = 1f;
    }

    private void SetCanvasGroupAlpha(CanvasGroup canvasGroup, float alpha)
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = alpha;
        }
    }
}
