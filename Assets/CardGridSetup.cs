using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CardGridSetup : MonoBehaviour
{
    public GameObject cardPrefab;       // Prefab for the card
    public Transform gridParent;        // Parent panel with Grid Layout Group
    public int rows = 2;                // Number of rows
    public int columns = 5;             // Number of columns
    public Sprite cardBackImage;        // Card back sprite (optional)
    [Header("Card Pair Data")]
    public List<Sprite> professionSprites;   // List of professions (front images)
    public List<Sprite> placeSprites;        // List of places (front images)

    [Header("Sound Effects")]
    public AudioSource audioSource;     // Audio source to play sounds
    public AudioClip flipSound;         // Sound for flipping cards
    public List<AudioClip> matchSound;        // Sound for matching cards
    public AudioClip wrongMatchSound;   // Sound for wrong match
    public AudioClip winSound;          // Sound for winning

    [Header("Panel Management")]
    public GameObject rewardPanel;   // Panels to show after completion
    public GameObject playAgainPanel;   // Panel to show play again option

    private int totalMatchedPairs = 0;

    public List<CardBehavior> flippedCards = new List<CardBehavior>();
    public bool isProcessing = false;

    void Start()
    {
        // If no audio source is assigned, try to get one from this GameObject
        // Ensure audio source is available
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
        }
        GenerateCardGrid();
    }

    void GenerateCardGrid()
    {
        if (professionSprites.Count != placeSprites.Count)
        {
            Debug.LogError("Professions and places must have the same number of elements!");
            return;
        }
        // Ensure the Grid Layout Group is set up
        GridLayoutGroup grid = gridParent.GetComponent<GridLayoutGroup>();
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = columns;

        var cardPairs = new List<(int pairId, Sprite sprite)>();

        // Instantiate cards in a grid pattern
        // Populate card pairs
        for (int i = 0; i < professionSprites.Count; i++)
        {
            cardPairs.Add((i, professionSprites[i]));
            cardPairs.Add((i, placeSprites[i]));
        }

        Shuffle(cardPairs);
        int cardCount = Mathf.Min(cardPairs.Count, rows * columns);


        for (int i = 0; i < rows * columns; i++)
        {
            GameObject newCard = Instantiate(cardPrefab, gridParent);
            CardBehavior cardBehavior = newCard.GetComponent<CardBehavior>();

            // Set the card ID and front image
            cardBehavior.cardID = cardPairs[i].pairId;
            cardBehavior.SetFrontImage(cardPairs[i].sprite);
            // Set card back sprite
            Image backImage = newCard.transform.Find("Back").GetComponent<Image>();
            cardBehavior.SetBackImage(cardBackImage ?? backImage.sprite);

            Button button = newCard.GetComponent<Button>();
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => OnCardClicked(cardBehavior));
        }
    }

    public void OnCardClicked(CardBehavior card)
    {
        if (isProcessing || !card.GetComponent<Button>().interactable)
            return;

        // Play card flip sound
        PlayFlipSound();

        card.FlipCard();
    }

    public void PlayFlipSound()
    {
        if (flipSound != null)
        {
            audioSource.PlayOneShot(flipSound);
        }
    }

    public void PlayMatchSound()
    {
        if (matchSound.Count > 0)
        {
            audioSource.PlayOneShot(matchSound[Random.Range(0, matchSound.Count)]);
        }
    }

    public void PlayWrongMatchSound()
    {
        if (wrongMatchSound != null)
        {
            audioSource.PlayOneShot(wrongMatchSound);
        }
    }

    public void AddFlippedCard(CardBehavior card)
    {
        flippedCards.Add(card);

        if (flippedCards.Count == 2)
        {
            StartCoroutine(CheckMatch());
        }
    }

    private IEnumerator CheckMatch()
    {
        isProcessing = true;

        // Wait for a moment to allow the player to see both cards
        yield return new WaitForSeconds(1f);

        if (flippedCards[0].cardID == flippedCards[1].cardID)
        {
            flippedCards[0].PlayAnimation();
            flippedCards[1].PlayAnimation();
            GlobalCounter.IncrementarAciertosCartas();
            // Cards match; keep them flipped
            flippedCards[0].gameObject.GetComponent<Button>().interactable = false;
            flippedCards[1].gameObject.GetComponent<Button>().interactable = false;
            totalMatchedPairs++;
            if (totalMatchedPairs == professionSprites.Count)
            {
                Debug.Log("END GAME");
                yield return new WaitForSeconds(1.5f);
                yield return StartCoroutine(HandleGameCompletion());
            }
        }
        else
        {
            GlobalCounter.IncrementarNoAciertos();
            // Cards don't match; flip them back
            flippedCards[0].ResetCard();
            flippedCards[1].ResetCard();
        }

        flippedCards.Clear();
        isProcessing = false;
    }

    private IEnumerator HandleGameCompletion()
    {
        // Wait a moment before showing completion
        yield return new WaitForSeconds(1f);

        // Show completion panel
        if (rewardPanel != null)
        {
            rewardPanel.SetActive(true);
        }

        yield return new WaitForSeconds(5f);

        ShowPlayAgainPanel();
    }

    private void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            T temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }

    // Method to restart the game
    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Method to show play again panel
    public void ShowPlayAgainPanel()
    {
        if (playAgainPanel != null)
        {
            playAgainPanel.SetActive(true);
        }
    }

}