using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public void FadeOutAndBack(GameObject personaje, System.Action onFadeComplete = null)
    {
        StartCoroutine(FadeOutAndBackCoroutine(personaje, onFadeComplete));
    }

    private IEnumerator FadeOutAndBackCoroutine(GameObject personaje, System.Action onFadeComplete)
    {
        SpriteRenderer spriteRenderer = personaje.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            // Fade out
            for (float t = 0; t < 0.5f; t += Time.deltaTime)
            {
                Color color = spriteRenderer.color;
                color.a = Mathf.Lerp(1, 0, t / 0.5f); // Cambia de 1 a 0
                spriteRenderer.color = color;
                yield return null;
            }

            // Asegúrate de que la opacidad sea 0
            Color finalColor = spriteRenderer.color;
            finalColor.a = 0;
            spriteRenderer.color = finalColor;

            // Espera 1 segundo
            yield return new WaitForSeconds(1f);

            // Fade in
            for (float t = 0; t < 0.5f; t += Time.deltaTime)
            {
                Color color = spriteRenderer.color;
                color.a = Mathf.Lerp(0, 1, t / 0.5f); // Cambia de 0 a 1
                spriteRenderer.color = color;
                yield return null;
            }

            // Asegúrate de que la opacidad sea 1
            finalColor = spriteRenderer.color;
            finalColor.a = 1;
            spriteRenderer.color = finalColor;

            // Llama al callback si no es null
            onFadeComplete?.Invoke();
        }
    }
}
