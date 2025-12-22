using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class charter_Erupcion : MonoBehaviour
{
    public float speed = 5f;
    private Vector2 targetPosition;
    private bool isMoving;
    private bool firstClick = true;
    private Animator animator;
    private Rigidbody2D rb;
    private int clickCount = 0;

    public List<Vector2> waypoints;
    private int currentWaypointIndex = 0;

    public Vector2 DestinationInorrecto;
    public Vector2 DestinationEmpezando;
    public Button botones;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip clickSound;
    public AudioClip[] randomSounds;
    public AudioSource audioWalkCorrect1;
    public AudioSource audioWalkInCorrect1;

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        isMoving = false;
        animator.SetBool("isWalking", false);

        botones.interactable = false;

    }

    void Update()
    {
        GameObject btnAction = EventSystem.current.currentSelectedGameObject;

        if (Input.GetMouseButtonDown(0) && btnAction != null && btnAction.GetComponent<Button>() != null)
        {
            Debug.Log("Boton UI Presionado");
            return;
        }
        if (Input.GetMouseButtonDown(0) && firstClick)
        {
            animator.SetBool("isMoving", true);
            firstClick = false;
        }

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

            if (hit)
            {
                // Detectar si se hace clic en los cuadrados
                if (hit.collider.CompareTag("Naturaleza"))
                {
                    PlayToAudioClicIncorrect();
                    return;
                }

                HandleClick(hit.collider);
            }
        }

        if (isMoving)
        {
            MoveCharacter();
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


    void HandleClick(Collider2D hit)
    {
        if (hit.CompareTag("caminoCorrecto"))
        {
            if (waypoints.Count > 0)
            {
                currentWaypointIndex = 0;
                targetPosition = waypoints[currentWaypointIndex];
                isMoving = true;
            }

            botones.interactable = true;
        }
        else if (hit.CompareTag("caminoIncorrecto"))
        {
            targetPosition = DestinationInorrecto;
            isMoving = true;
            PlayTheAudioCorrect(audioWalkInCorrect1);
        }
        else if (hit.CompareTag("empezandoCaminar"))
        {
            targetPosition = DestinationEmpezando;
            Debug.Log("Empiece a elegir el camino correcto");
            isMoving = true;
        }
        else
        {
            Debug.Log("Lugar incorrecto");
            return;
        }
    }

    void MoveCharacter()
    {
        Vector2 newPosition = Vector2.MoveTowards(rb.position, targetPosition, speed * Time.deltaTime);
        rb.MovePosition(newPosition);

        UpdateDirection(newPosition);

        // Comprobar si se ha alcanzado el targetPosition
        if (Vector2.Distance(rb.position, targetPosition) < 0.1f)
        {
            if (targetPosition == DestinationInorrecto)
            {
                targetPosition = DestinationEmpezando;
                Debug.Log("Regresando al punto de inicio...");
            }
            else if (currentWaypointIndex < waypoints.Count - 1 && targetPosition == waypoints[currentWaypointIndex])
            {
                currentWaypointIndex++;
                targetPosition = waypoints[currentWaypointIndex];
            }
            else
            {
                // Si hemos llegado al �ltimo waypoint
                isMoving = false;
                animator.SetBool("isWalking", false);
                ShowDestinationMessage(targetPosition);
            }
        }
        else
        {
            animator.SetBool("isWalking", true);
        }
    }

    private void ShowDestinationMessage(Vector2 position)
    {
        if (currentWaypointIndex == waypoints.Count - 1)
        {
            PlayTheAudioCorrect(audioWalkCorrect1);

            if (GameManagerOsiPersonaje.Instance != null)
            {
                GameManagerOsiPersonaje.Instance.IncrementScore();
            }

            StartCoroutine(LoadSceneWithDelay("recompensa3_juego2", 3f));
        }
    }

    private IEnumerator LoadSceneWithDelay(string sceneName, float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadSceneAsync(sceneName);
    }

    private void UpdateDirection(Vector2 newPosition)
    {
        Vector2 direction = (newPosition - rb.position).normalized;

        // Use a threshold to determine when we're "close enough" to cardinal directions
        float threshold = 0.7f; // You can adjust this value between 0.5 and 0.9

        if (Mathf.Abs(direction.x) > threshold)
        {
            // Pure horizontal movement
            animator.SetFloat("Horizontal", Mathf.Sign(direction.x));
            animator.SetFloat("Vertical", 0f);
        }
        else if (Mathf.Abs(direction.y) > threshold)
        {
            // Pure vertical movement
            animator.SetFloat("Horizontal", 0f);
            animator.SetFloat("Vertical", Mathf.Sign(direction.y));
        }
        else
        {
            // For diagonal movement, choose based on which direction is larger
            if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
            {
                animator.SetFloat("Horizontal", Mathf.Sign(direction.x));
                animator.SetFloat("Vertical", 0f);
            }
            else
            {
                animator.SetFloat("Horizontal", 0f);
                animator.SetFloat("Vertical", Mathf.Sign(direction.y));
            }
        }
    }
}
