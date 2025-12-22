using UnityEngine;
using UnityEngine.EventSystems;

public class CustomCursor : MonoBehaviour
{
    public bool useMouse = true;
    public RectTransform cursorTransform;

    private void Start()
    {
        // Oculta el puntero del sistema
        Cursor.visible = false;
    }

    private void Update()
    {
        if (useMouse)
        {
            HandleMouseInput();
        }
        else
        {
            HandleTouchInput();
        }
    }

    private void HandleMouseInput()
    {
        Vector2 cursorPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            (RectTransform)cursorTransform.parent,
            Input.mousePosition,
            null,
            out cursorPos
        );
        cursorTransform.localPosition = cursorPos;

        // Manejar clic izquierdo del ratón
        if (Input.GetMouseButtonDown(0))
        {
            PerformClick(Input.mousePosition);
        }
        // Manejar clic derecho del ratón
        if (Input.GetMouseButtonDown(1))
        {
            PerformRightClick(Input.mousePosition);
        }
    }

    private void HandleTouchInput()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            Vector3 touchPosition = touch.position;
            touchPosition.z = 10;
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(touchPosition);

            cursorTransform.position = worldPosition;

            // Manejar toque
            if (touch.phase == TouchPhase.Began)
            {
                PerformClick(touch.position);
            }
        }
    }

    private void PerformClick(Vector3 screenPosition)
    {
        if (EventSystem.current == null)
        {
            Debug.LogError("EventSystem.current is null. Make sure there is an EventSystem in the scene.");
            return;
        }

        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = screenPosition
        };

        var raycastResults = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, raycastResults);

        Debug.Log("Raycast Results Count: " + raycastResults.Count);

        foreach (var result in raycastResults)
        {
            GameObject clickedObject = result.gameObject;
            var clickHandler = clickedObject.GetComponent<IPointerClickHandler>();

            if (clickHandler != null)
            {
                clickHandler.OnPointerClick(pointerData);
                Debug.Log("Click Handled for: " + clickedObject.name);
                return;
            }
            else
            {
                Debug.LogWarning("No IPointerClickHandler found on: " + clickedObject.name);
            }
        }

        Debug.LogWarning("No UI elements with IPointerClickHandler found under click position.");
    }

    private void PerformRightClick(Vector3 screenPosition)
    {
        Debug.Log("Right Click Detected at: " + screenPosition);
    }
}
