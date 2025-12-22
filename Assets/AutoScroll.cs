using UnityEngine;
using UnityEngine.EventSystems;

public class AutoScroll : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public RectTransform contentPanel;
    public float scrollSpeed = 50f;
    private bool isScrollingRight = true;
    private bool isHovered = false;

    void Update()
    {
        if (isHovered || contentPanel == null) return;

        Vector3 movement = isScrollingRight
            ? Vector3.left * scrollSpeed * Time.deltaTime
            : Vector3.right * scrollSpeed * Time.deltaTime;

        contentPanel.localPosition += movement;

        // Check for boundaries to reverse the direction
        if (contentPanel.localPosition.x <= -contentPanel.rect.width / 2)
        {
            isScrollingRight = false;
        }
        else if (contentPanel.localPosition.x >= contentPanel.rect.width / 2)
        {
            isScrollingRight = true;
        }
    }

    // Pause scrolling when hovered
    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
    }

    // Resume scrolling when not hovered
    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
    }
}
