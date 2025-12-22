using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PanelManagerTransitionLugaresPublicos : MonoBehaviour
{
    public GameObject[] instructionPanels; // Array to hold all instruction panels for different games
    public AudioSource[] audioButtons; // Array to hold audio sources for buttons

    public string[] scenes;
    public float transitionDuration = 1f;

    private AudioSource activeAudioSource;
    private bool isSkipping = false;
    private Coroutine currentTransitionCoroutine;
    private int currentSceneIndex = 0;
    private bool isInstructionPanel = false;

    private void Start()
    {
        // Deactivate all instruction panels initially
        foreach (var panel in instructionPanels)
        {
            panel.SetActive(false);
        }
    }

    private void Update()
    {
        if (isInstructionPanel && Input.GetMouseButtonDown(0) && !isSkipping)
        {
            SkipToNextScene();
        }
    }

    private void SkipToNextScene()
    {
        isSkipping = true;

        // Stop the currently playing audio
        if (activeAudioSource != null && activeAudioSource.isPlaying)
        {
            activeAudioSource.Stop();
        }

        // Stop any running coroutine
        if (currentTransitionCoroutine != null)
        {
            StopCoroutine(currentTransitionCoroutine);
        }

        // Immediately load the next scene
        SceneManager.LoadScene(scenes[currentSceneIndex]);
    }

    public void OnButtonClick(int buttonIndex)
    {
        if (buttonIndex < 0 || buttonIndex >= scenes.Length || buttonIndex >= instructionPanels.Length)
        {
            Debug.LogError("Invalid button index");
            return;
        }
        Debug.Log("Button index: " + buttonIndex);
        // Activate the corresponding instruction panel
        for (int i = 0; i < instructionPanels.Length; i++)
        {
            instructionPanels[i].SetActive(i == buttonIndex);
        }

        // Play the corresponding button audio
        if (audioButtons != null && buttonIndex < audioButtons.Length)
        {
            activeAudioSource = audioButtons[0];
            if (activeAudioSource != null)
            {
                activeAudioSource.Play();
            }
        }

        // Start the transition for the selected panel
        currentTransitionCoroutine = StartCoroutine(ShowInstructionPanel(buttonIndex));
    }

    private IEnumerator ShowInstructionPanel(int panelIndex)
    {
        GameObject panel = instructionPanels[panelIndex];
        CanvasGroup canvasGroup = panel.GetComponent<CanvasGroup>();
        AudioSource instructionAudio = panel.GetComponent<AudioSource>();

        if (canvasGroup != null)
        {
            float elapsedTime = 0f;

            // Fade in the panel
            while (elapsedTime < transitionDuration)
            {
                elapsedTime += Time.deltaTime;
                canvasGroup.alpha = Mathf.Clamp01(elapsedTime / transitionDuration);
                yield return null;
            }

            canvasGroup.alpha = 1f;
        }

        // Play instruction audio
        if (instructionAudio != null && !isSkipping)
        {
            activeAudioSource = instructionAudio;
            instructionAudio.Play();
        }

        // Optional: Wait for the instruction audio to finish
        while (instructionAudio != null && instructionAudio.isPlaying && !isSkipping)
        {
            yield return null;
        }

        if (!isSkipping)
        {
            // Load the scene after the instruction panel
            SceneManager.LoadScene(scenes[panelIndex]);
        }
    }
}
