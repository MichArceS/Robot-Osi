using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CharacterMovementOsiMochila : MonoBehaviour
{

    private int currentWaypointIndex = 0;
    private float speed = 5f;
    private bool isMoving = false;
    private bool hasReachedMama = false;
    private bool hasReachedCharacter = false;
    private Animator mamaAnimator;
    private Animator newCharacter1Animator;
    private Animator newCharacter2Animator;
    private Animator animator;
    private Rigidbody2D rb;
    private int clickCount = 0;

    public List<Button> botones;
    public Vector2 destination;
    public Vector2 destinationMama;
    public Transform mamaTransform;
    public Vector2[] waypoints;
    public Vector2[] mamaWaypoints;
    public GameObject newCharacter1;
    public GameObject newCharacter2;


    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip clickSound;
    public AudioClip[] randomSounds;
    public AudioSource audioMama;
    public AudioSource audioSalida;

    private bool isAtMama = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        mamaAnimator = mamaTransform.GetComponent<Animator>();
        newCharacter1Animator = newCharacter1.GetComponent<Animator>();
        newCharacter2Animator = newCharacter2.GetComponent<Animator>();
        StartCoroutine(StartMovement());

        foreach (Button boton in botones)
        {
            if (boton != null)
            {
                boton.interactable = false;
            }
        }
    }

    private IEnumerator StartMovement()
    {
        yield return new WaitForSeconds(2f);
        isMoving = true;
    }

    void Update()
    {
        HandleMovement();
        HandleClickInput();
    }

    private void HandleMovement()
    {
        if (isMoving && currentWaypointIndex < waypoints.Length)
        {
            Vector2 target = waypoints[currentWaypointIndex];
            Vector2 direction = (target - rb.position).normalized;
            rb.linearVelocity = direction * speed;
            animator.SetFloat("Horizontal", direction.x);
            animator.SetFloat("Vertical", direction.y);
            animator.SetBool("isWalking", true);
            if (Vector2.Distance(rb.position, target) < 0.1f)
            {
                rb.linearVelocity = Vector2.zero;
                currentWaypointIndex++;
                CheckArrival();
            }
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
            animator.SetBool("isWalking", false);
        }
    }

    private void CheckArrival()
    {
        if (currentWaypointIndex >= waypoints.Length)
        {
            if (waypoints == mamaWaypoints)
            {
                isAtMama = true;
                PlayTheAudioCorrect(audioMama);
                if (botones.Count > 0 && botones[0] != null)
                {
                    botones[0].interactable = true;
                }
                //ActivateButton();
                //hasReachedCharacter = true;
                Debug.Log("Has llegado a la mama!");
            }
            else if (destination == waypoints[currentWaypointIndex - 1])
            {
                hasReachedCharacter = true;
                CheckBothArrived();
            }
        }
    }

    private void HandleClickInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
            if (hit.collider != null)
            {
                ProcessHit(hit);
            }
        }
    }

    private void ProcessHit(RaycastHit2D hit)
    {
        if (hit.collider.CompareTag("Pared"))
        {
            Debug.Log("�All� no puedes caminar!");
            PlayToAudioClicIncorrect();
        }
        else if (hit.collider.CompareTag("Mama"))
        {
            if (!isAtMama)
            {
                waypoints = mamaWaypoints;
                currentWaypointIndex = 0;
                isMoving = true;
            }
        }
        else if (hit.collider.CompareTag("PuertaSalida") && isAtMama)
        {
            Debug.Log("Puerta clickeada, moviendo a salida");
            ActivateButton();
        }
        else
        {
            Vector2 newWaypoint = new Vector2(hit.point.x, hit.point.y + 2f);
            waypoints = new Vector2[] { newWaypoint };
            currentWaypointIndex = 0;
            isMoving = true;
        }
    }

    private void PlayTheAudioCorrect(AudioSource audioSource)
    {
        if (audioSource != null)
        {
            audioSource.Play();
        }
    }

    private void PlayToAudioClicIncorrect()
    {
        clickCount++;

        if (clickCount <= 2)
        {
            PlayAudio(clickSound);
        }
        else if (clickCount > 2)
        {
            PlayRandomSound();
        }
    }

    private void PlayAudio(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.clip = clip;
            audioSource.Play();
        }
    }

    private void PlayRandomSound()
    {
        if (audioSource != null && randomSounds.Length > 0)
        {
            AudioClip randomClip = randomSounds[Random.Range(0, randomSounds.Length)];
            PlayAudio(randomClip);
        }
    }


    public void ActivateButton()
    {
        StartCoroutine(MoveToDestinationAfterDelay(0.5f));
    }

    private IEnumerator MoveToDestinationAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        waypoints = new Vector2[] { destination };
        currentWaypointIndex = 0;
        isMoving = true;
        yield return StartCoroutine(MoveMamaToDestination(destinationMama));
    }

    private IEnumerator MoveMamaToDestination(Vector2 newDestination)
    {
        Vector2 adjustedDestination = newDestination + (newDestination - (Vector2)mamaTransform.position).normalized * 0.5f;
        while (Vector2.Distance(mamaTransform.position, adjustedDestination) > 0.1f)
        {
            Vector2 direction = (adjustedDestination - (Vector2)mamaTransform.position).normalized;
            mamaTransform.position += (Vector3)(direction * speed * Time.deltaTime);
            mamaAnimator.SetFloat("Horizontal", direction.x);
            mamaAnimator.SetFloat("Vertical", direction.y);
            mamaAnimator.SetBool("isMoving", true);
            yield return null;
        }
        mamaTransform.position = destinationMama;
        hasReachedMama = true;
        mamaAnimator.SetBool("isMoving", false);
        CheckBothArrived();
    }

    private void CheckBothArrived()
    {
        if (hasReachedMama && hasReachedCharacter)
        {
            Debug.Log("�Ambos han llegado a sus destinos!");
            PlayTheAudioCorrect(audioSalida);
            FadeOutCharacter(animator);
            FadeOutCharacter(mamaAnimator);
            ActivateNewCharacters();
        }
    }

    private void FadeOutCharacter(Animator characterAnimator)
    {
        SpriteRenderer spriteRenderer = characterAnimator.GetComponent<SpriteRenderer>();
        Color color = spriteRenderer.color;
        color.a = 0f;
        spriteRenderer.color = color;
    }

    private void ActivateNewCharacters()
    {
        newCharacter1.SetActive(true);
        newCharacter2.SetActive(true);
        if (botones.Count > 0 && botones[1] != null)
        {
            botones[1].interactable = true;
        }
        StartCoroutine(WaitForNewCharacterAnimations());
    }

    private IEnumerator WaitForNewCharacterAnimations()
    {
        yield return new WaitForSeconds(newCharacter2Animator.GetCurrentAnimatorStateInfo(0).length);

        SceneManager.LoadSceneAsync("terremoto_juego2");
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Pared"))
        {
            isMoving = false;
            animator.SetBool("isWalking", false);
        }
    }
}
