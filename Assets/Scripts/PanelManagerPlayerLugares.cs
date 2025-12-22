using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine.UI;

public class PanelManagerPlayerLugares : MonoBehaviour
{
    public GameObject[] panels;
    public GameObject[] panelsContent;
    public GameObject[] panelsRecompensas;
    public GameObject additionalPanel;
    public float delayBeforeNextPanel = 3f;
    public float rewardPanelDuration = 3f;
    public string sceneOnBackButton;
    public AudioSource buttonClickAudio;

    private List<int> availablePanelIndices;
    private Stack<int> panelHistory;
    private int currentPanelIndex = -1;
    private int[] aciertosPorPanel;
    private bool allPanelsCompleted = false;
    private AudioSource currentAudioSource;
    public AudioClip[] audiosLugares;
    public AudioSource correctAudio;
    private AudioSource audioRepetir;
    public RuntimeAnimatorController correctAnimator;
    public RuntimeAnimatorController wrongAnimator;

    public Sprite[] optionImages;

    private bool isPlayingAudio = false;

    void Start()
    {
        aciertosPorPanel = new int[panels.Length];
        for (int i = 0; i < aciertosPorPanel.Length; i++)
        {
            aciertosPorPanel[i] = 1;
        }
        InitializePanelIndices();
        panelHistory = new Stack<int>();
        ShowNextPanel();
    }

    public void PlayButtonClickAudio()
    {
        if (buttonClickAudio != null)
        {
            buttonClickAudio.Play();
        }
    }

    public void RepeatAudio()
    {
        PlayButtonClickAudio();
        if (currentAudioSource != null)
        {
            currentAudioSource.Stop();
            currentAudioSource.Play();
        }
    }

    void OnDestroy()
    {
    }

    private void InitializePanelIndices()
    {
        availablePanelIndices = new List<int>();
        for (int i = 0; i < panels.Length; i++)
        {
            availablePanelIndices.Add(i);
        }
        allPanelsCompleted = false;
    }

    public void SetCurrentAudioSource(AudioSource audioSource)
    {
        currentAudioSource = audioSource;
    }

    public void SetCurrentAudioSourceAndPlay(AudioSource audioSource)
    {
        SetCurrentAudioSource(audioSource);
        audioSource.Play();
    }

    private IEnumerator FindAudioSourceInPanel(int panelIndex)
    {
        yield return new WaitForSeconds(0.1f);
        AudioSource[] audioSources = panels[panelIndex].GetComponentsInChildren<AudioSource>();
        audioRepetir = null;
        if (audioSources.Length > 0)
        {
            audioRepetir = audioSources[0];
        }
        if (audioRepetir != null)
        {
            SetCurrentAudioSource(audioRepetir);
            audioRepetir.Play();
        }
    }

    public void PlayCurrentAudio()
    {
        if (audioRepetir != null)
        {
            audioRepetir.Play();
        }
    }


    private void ShowNextPanel()
    {
        if (availablePanelIndices.Count == 0)
        {
            if (!allPanelsCompleted)
            {
                allPanelsCompleted = true;
                Debug.Log("ENDING");
                Debug.Log(panelsRecompensas[0]);
                StartCoroutine(ShowRewardPanel(0));
            }
            return;
        }

        foreach (GameObject panel in panels)
        {
            panel.SetActive(false);
        }

        int randomIndex = Random.Range(0, availablePanelIndices.Count);
        int panelIndex = availablePanelIndices[randomIndex];
        panels[panelIndex].SetActive(true);
        int correctAnswer = panelIndex; // Assuming the correct answer is based on the panel index
        Sprite correctImage = optionImages[correctAnswer];
        AudioClip audioCorrect = audiosLugares[correctAnswer];
        Transform itemsParent = panelsContent[panelIndex].transform;
        if (itemsParent != null)
        {
            List<Transform> items = new List<Transform>();
            foreach (Transform item in itemsParent)
            {
                items.Add(item);
            }

            // Load all images into each item image component
            for (int i = 0; i < items.Count; i++)
            {
                Image itemImage = items[i].GetComponent<Image>();
                if (itemImage != null)
                {
                    itemImage.sprite = optionImages[i];
                }
            }

            // Shuffle the items
            for (int i = items.Count - 1; i > 0; i--)
            {
                int randIndex = Random.Range(0, i + 1);
                Transform temp = items[i];
                items[i] = items[randIndex];
                items[randIndex] = temp;
            }

            // Assign the shuffled items back to the parent
            for (int i = 0; i < items.Count; i++)
            {
                items[i].SetSiblingIndex(i);
            }

            // Assign interaction events
            for (int i = 0; i < items.Count; i++)
            {
                Image itemImage = items[i].GetComponent<Image>();
                if (itemImage != null)
                {
                    Button itemButton = items[i].GetComponent<Button>();
                    if (itemButton != null)
                    {
                        if (itemImage.sprite == correctImage)
                        {
                            Animator childAnimator = itemButton.GetComponentInChildren<Animator>();
                            if (childAnimator != null)
                            {
                                childAnimator.runtimeAnimatorController = correctAnimator;
                                Debug.Log("Correct Animator: " + correctAnimator);
                                itemButton.onClick.AddListener(() => { childAnimator.Play("ConfettiCorrect"); HandleCorrectAnswer(); });
                            }
                        }
                        else
                        {
                            Animator childAnimator = itemButton.GetComponentInChildren<Animator>();
                            if (childAnimator != null)
                            {
                                childAnimator.runtimeAnimatorController = wrongAnimator;
                                Debug.Log("Correct Animator: " + correctAnimator);
                                itemButton.onClick.AddListener(() => { childAnimator.Play("ConfettiWrong"); HandleWrongAnswer(); });
                            }
                        }
                    }
                }
            }
        }
        correctAudio.clip = audioCorrect;
        currentPanelIndex = panelIndex;
        panelHistory.Push(panelIndex);
        GlobalCounter.ResetCounters();
        StartCoroutine(FindAudioSourceInPanel(currentPanelIndex));
        availablePanelIndices.RemoveAt(randomIndex);
    }

    private IEnumerator PlayCorrectAudio()
    {
        if (isPlayingAudio) yield break;
        isPlayingAudio = true;

        if (correctAudio != null && correctAudio.clip != null)
        {
            correctAudio.Play();
            Debug.Log("Playing correct audio", correctAudio);
            yield return new WaitForSeconds(correctAudio.clip.length);
        }
        else
        {
            Debug.LogWarning("No correct audio found");
        }

        yield return new WaitForSeconds(1.5f);
        isPlayingAudio = false;
    }

    private void HandleCorrectAnswer()
    {
        PlayButtonClickAudio();
        Animator panelAnimator = panels[currentPanelIndex].GetComponent<Animator>();
        if (panelAnimator != null)
        {
            panelAnimator.SetTrigger("CorrectAnswer");
        }
        Debug.Log("Correct Answer");
        StartCoroutine(PlayCorrectAudio());
        StartCoroutine(waitTenSeconds());
        GlobalCounter.IncrementarAciertosJuego3();
        ShowNextPanelAfterDelay(delayBeforeNextPanel);
    }

    private IEnumerator waitTenSeconds()
    {
        yield return new WaitForSeconds(10);
    }

    private void HandleWrongAnswer()
    {
        PlayButtonClickAudio();
        GlobalCounter.IncrementarNoAciertosJuego3();
    }

    public void ShowPreviousPanel()
    {
        PlayButtonClickAudio();
        if (panelHistory.Count > 1)
        {
            panelHistory.Pop();
            int previousPanelIndex = panelHistory.Peek();
            ShowPanel(previousPanelIndex);
            ResetPanelObjects();
        }
        else
        {
            PlayButtonClickAudio();
            SceneManager.LoadSceneAsync(sceneOnBackButton);
        }
    }

    public void ShowNextPanelAfterDelay(float delay)
    {
        StartCoroutine(ChangePanelAfterDelay(delay));
    }

    private IEnumerator ChangePanelAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ShowNextPanel();
    }

    private void ShowPanel(int index)
    {
        if (index < 0 || index >= panels.Length)
        {
            return;
        }

        for (int i = 0; i < panels.Length; i++)
        {
            panels[i].SetActive(i == index);
        }

        currentPanelIndex = index;
        GlobalCounter.ResetCounters();
        StartCoroutine(FindAudioSourceInPanel(currentPanelIndex));
    }

    private void ResetPanelObjects()
    {
        if (currentPanelIndex < 0 || currentPanelIndex >= panels.Length)
        {
            return;
        }

        GameObject currentPanel = panels[currentPanelIndex];
        Transform almarioTransform = currentPanel.transform.Find("Almario");
        if (almarioTransform == null)
        {
            return;
        }

        foreach (Transform child in almarioTransform)
        {
            if (child.CompareTag("Draggable"))
            {
                var draggable = child.GetComponent<ControllerDrag_and_Drop>();
                if (draggable != null)
                {
                    draggable.ResetState();
                }
            }
        }
    }

    public void RestartScene()
    {
        PlayButtonClickAudio();
        //string currentSceneName = SceneManager.GetActiveScene().name;
        //SceneManager.LoadScene(currentSceneName);
        SceneManager.LoadSceneAsync("lugares_publicos_juego");
    }
    public void BackFirstScene()
    {
        PlayButtonClickAudio();
        SceneManager.LoadSceneAsync("Pantalla_Actividades");
    }

    public void LoadNewScene()
    {
        if (!string.IsNullOrEmpty(sceneOnBackButton))
        {
            SceneManager.LoadScene(sceneOnBackButton);
        }
    }

    private void ShowRewardPanelAndNext()
    {
        if (currentPanelIndex < panelsRecompensas.Length)
        {
            StartCoroutine(ShowRewardPanel(currentPanelIndex));
        }
        else
        {
            if (additionalPanel != null)
            {
                additionalPanel.SetActive(true);
            }
        }
    }

    private IEnumerator ShowRewardPanel(int panelIndex)
    {
        panelsRecompensas[0].SetActive(true);
        AudioSource[] audios = panelsRecompensas[0].GetComponentsInChildren<AudioSource>();
        audios[0].Play();
        yield return new WaitUntil(() => !audios[0].isPlaying);
        yield return new WaitForSeconds(rewardPanelDuration);

        if (additionalPanel != null)
        {
            additionalPanel.SetActive(true);
        }
    }
}