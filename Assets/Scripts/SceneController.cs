using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class SceneController : MonoBehaviour
{
    public string sceneLoadName;
    public TextMeshProUGUI textProgress;
    public Slider sliderProgress;
    private AsyncOperation loadAsync;

    void Start()
    {
        StartCoroutine(LoadScene(sceneLoadName));
    }

    private IEnumerator LoadScene(string nameToLoad)
    {
        textProgress.text = "Cargando... 00%";
        sliderProgress.value = 0f;

        loadAsync = SceneManager.LoadSceneAsync(nameToLoad);
        loadAsync.allowSceneActivation = false;

   
        float loadDuration = 5.0f;
        float elapsedTime = 0f;

        while (elapsedTime < loadDuration)
        {
            
            float progress = elapsedTime / loadDuration;

            
            textProgress.text = "Cargando... " + (progress * 100f).ToString("00") + "%";
            sliderProgress.value = progress;

            
            elapsedTime += Time.deltaTime;

            yield return null;
        }


        yield return new WaitForSeconds(0.3f);

        if (!loadAsync.allowSceneActivation)
        {
            loadAsync.allowSceneActivation = true;
            Debug.Log("Escena activada manualmente");
        }
    }
}