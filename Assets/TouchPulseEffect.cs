using UnityEngine;

public class TouchPulseEffect : MonoBehaviour
{
    [Header("Configuración del Pulso")]
    [SerializeField] public GameObject pulsePrefab;  // Prefab del círculo que se usará como pulso
    [SerializeField] private float maxScale = 2f;     // Escala máxima del pulso
    [SerializeField] private float duration = 1f;     // Duración de la animación
    [SerializeField] private float fadeSpeed = 2f;    // Velocidad de desvanecimiento

    void Update()
    {
        // Verificar si hay un toque en la pantalla
        if (Input.touchCount > 0)
        {
            // Obtener el primer toque
            Touch touch = Input.GetTouch(0);

            // Verificar si el toque acaba de comenzar
            if (touch.phase == TouchPhase.Began)
            {
                CreatePulse(touch.position);
            }
        }

        // Alternativa para pruebas con el mouse
        if (Input.GetMouseButtonDown(0))
        {
            CreatePulse(Input.mousePosition);
        }
    }

    void CreatePulse(Vector2 position)
    {
        // Convertir la posición de la pantalla a posición en el mundo
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(position.x, position.y, 10f));

        // Instanciar el prefab del pulso
        GameObject pulse = Instantiate(pulsePrefab, worldPos, Quaternion.identity);

        // Obtener el componente SpriteRenderer para manejar la transparencia
        SpriteRenderer spriteRenderer = pulse.GetComponent<SpriteRenderer>();

        // Iniciar la corrutina de animación
        StartCoroutine(PulseAnimation(pulse, spriteRenderer));
    }

    System.Collections.IEnumerator PulseAnimation(GameObject pulse, SpriteRenderer spriteRenderer)
    {
        float elapsedTime = 0f;
        Vector3 initialScale = pulse.transform.localScale;
        Color initialColor = spriteRenderer.color;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / duration;

            // Escalar el pulso
            pulse.transform.localScale = initialScale * (1f + (maxScale - 1f) * progress);

            // Desvanecer el pulso
            spriteRenderer.color = new Color(
                initialColor.r,
                initialColor.g,
                initialColor.b,
                Mathf.Lerp(1f, 0f, progress * fadeSpeed)
            );

            yield return null;
        }

        // Destruir el objeto al finalizar la animación
        Destroy(pulse);
    }
}