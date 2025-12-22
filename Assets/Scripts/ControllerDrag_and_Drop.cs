using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ControllerDrag_and_Drop : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private Vector3 initialPosition;
    private Vector2 initialSizeDelta;
    private bool isDropped = false;
    public string tagNameObjet; 

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>();

        // Inicia la coroutine para esperar 0.5 segundos y luego capturar la posición
        StartCoroutine(CaptureInitialPositionAfterDelay(0.5f));
    }

    private IEnumerator CaptureInitialPositionAfterDelay(float delay)
    {
        // Espera el tiempo especificado
        yield return new WaitForSeconds(delay);

        // Almacena la posición local después de la espera
        initialPosition = rectTransform.anchoredPosition;
        initialSizeDelta = rectTransform.sizeDelta;

        // Imprime la posición inicial al comenzar el juego
        //Debug.Log("Posición inicial del objeto '" + gameObject.name + "': " + initialPosition);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // Cambia la visibilidad y la interactividad del objeto mientras se arrastra
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.8f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Mueve el objeto basado en la posición del cursor
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // Restaura la visibilidad y la interactividad del objeto al soltarlo
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;

        if (eventData.pointerCurrentRaycast.gameObject.CompareTag(tagNameObjet))
        {
            // Obtiene el RectTransform del área de destino
            RectTransform droppableRect = eventData.pointerCurrentRaycast.gameObject.GetComponent<RectTransform>();
            // Ajusta la posición y el tamaño del objeto arrastrado para que se alinee con el área de destino
            rectTransform.position = droppableRect.position;
            rectTransform.sizeDelta = droppableRect.sizeDelta;

            isDropped = true;

            if (tagNameObjet == "DroppableMedias")
            {
                // Desactiva el objeto con la etiqueta
                GameObject objetoDesactivar = GameObject.FindGameObjectWithTag("DroppableMedias");

                if (objetoDesactivar != null)
                {
                    objetoDesactivar.SetActive(false);
                }
            }

            // Incrementa el contador global de aciertos
            GlobalCounter.IncrementarAciertos();
        }
        else
        {
            // Si el objeto no se suelta en el área de destino, vuelve a la posición inicial
            rectTransform.anchoredPosition = initialPosition;
            GlobalCounter.IncrementarNoAciertos();
        }
    }

    private void Update()
    {
        // Desactiva el script si el objeto ya ha sido soltado
        if (isDropped)
        {
            canvasGroup.blocksRaycasts = false;
            enabled = false;
        }
    }

    public void ResetState()
    {
        //Debug.Log($"Reiniciando objeto: {gameObject.name}");
        rectTransform.anchoredPosition = initialPosition;
        rectTransform.sizeDelta = initialSizeDelta;
        isDropped = false;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;
        enabled = true;
    }
}
