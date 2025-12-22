using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ControladorAnimacion : MonoBehaviour
{
    private Animator animator;
    public string sceneToLoad;

    void Start()
    {
        animator = GetComponent<Animator>();
        StartCoroutine(PlayAnimation());
    }

    private IEnumerator PlayAnimation()
    {
        float animationDuration = animator.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(animationDuration);

        SceneManager.LoadSceneAsync(sceneToLoad);
    }
}
