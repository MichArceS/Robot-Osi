using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CardBehavior : MonoBehaviour
{
    public GameObject backImage;  // Reference to the back image
    public GameObject frontImage; // Reference to the front image
    public GameObject animationObject; // Reference to the animation object
    public int cardID;

    private bool isFlipped = false;
    private CardGridSetup gameManager;

    [Header("Flip Animation Settings")]
    public float flipDuration = 0.5f;   // Duration of the flip animation
    public AnimationCurve flipCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f); // Smoothness of the animation

    void Start()
    {
        backImage.SetActive(true);
        frontImage.SetActive(false);
        gameManager = FindObjectOfType<CardGridSetup>();
    }

    public void FlipCard()
    {
        if (isFlipped || gameManager.isProcessing) return; // Prevent flipping if already flipped or during processing

        StartCoroutine(AnimateCardFlip());
        gameManager.AddFlippedCard(this);
    }

    private IEnumerator AnimateCardReset()
    {
        // Track the current rotation
        Quaternion currentRotation = transform.localRotation;
        Quaternion halfFlipRotation = currentRotation * Quaternion.Euler(0, 90, 0);
        Quaternion startRotation = Quaternion.identity;

        float elapsedTime = 0;

        // First half of reset - rotate to 90 degrees and swap images
        while (elapsedTime < flipDuration / 2)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / (flipDuration / 2);
            transform.localRotation = Quaternion.Lerp(currentRotation, halfFlipRotation, flipCurve.Evaluate(t));
            yield return null;
        }

        // Swap back to original face
        SwapCardFace();

        // Second half of reset - complete the rotation back to start
        elapsedTime = 0;
        while (elapsedTime < flipDuration / 2)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / (flipDuration / 2);
            transform.localRotation = Quaternion.Lerp(halfFlipRotation, startRotation, flipCurve.Evaluate(t));
            yield return null;
        }

        // Ensure final rotation
        transform.localRotation = startRotation;
        transform.localScale = new Vector3(1, 1, 1); // Reset local scale to avoid flipping issues
    }

    private IEnumerator AnimateCardFlip()
    {
        // Track the initial rotation
        Quaternion startRotation = transform.localRotation;
        Quaternion halfFlipRotation = startRotation * Quaternion.Euler(0, 90, 0);
        Quaternion fullFlipRotation = startRotation * Quaternion.Euler(0, 360, 0);

        float elapsedTime = 0;

        // First half of flip - rotate to 90 degrees and swap images
        while (elapsedTime < flipDuration / 2)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / (flipDuration / 2);
            transform.localRotation = Quaternion.Lerp(startRotation, halfFlipRotation, flipCurve.Evaluate(t));
            yield return null;
        }

        // Swap images at the midpoint of the flip
        SwapCardFace();

        // Second half of flip - complete the rotation
        elapsedTime = 0;
        while (elapsedTime < flipDuration / 2)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / (flipDuration / 2);
            transform.localRotation = Quaternion.Lerp(halfFlipRotation, fullFlipRotation, flipCurve.Evaluate(t));
            yield return null;
        }
        // Ensure final rotation
        //transform.localRotation = fullFlipRotation;
    }

    private void SwapCardFace()
    {
        isFlipped = !isFlipped;
        frontImage.gameObject.SetActive(isFlipped);
        backImage.gameObject.SetActive(!isFlipped);
    }

    public void ResetCard()
    {
        StartCoroutine(AnimateCardReset());

    }

    public void SetFrontImage(Sprite image)
    {
        frontImage.GetComponent<Image>().sprite = image;
    }

    public void SetBackImage(Sprite image)
    {
        backImage.GetComponent<Image>().sprite = image;
    }

    public void PlayAnimation()
    {
        animationObject.SetActive(true);
        animationObject.GetComponent<Animator>().Play("ConfettiCorrect");
    }
}
