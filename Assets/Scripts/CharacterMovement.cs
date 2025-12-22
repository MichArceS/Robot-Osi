using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class CharacterMovement : MonoBehaviour
{
    public float speed = 5f;
    private Vector2 targetPosition;
    private bool isMoving;
    private bool firstClick = true;
    private Animator animator;
    private Rigidbody2D rb;

    public List<Vector2> waypoints;
    private int currentWaypointIndex = 0;

    public Vector2 DestinationCorrecto;
    public Vector2 DestinationInorrecto;
    public Vector2 DestinationEmpezando;
    public Vector2 DestinationIncorrectoSegundo;
    public Vector2 DestinationIncorrectoTercer;
    public Vector2 DestinationEmpezarTercer;
    public List<Button> botones;
    private int clickCount = 0;

    private HashSet<string> visited = new HashSet<string>();
    private string currentPath = "";
    private bool hasCompletedFirstPath = false;
    private bool hasCompletedSecondPath = false;

    public GameObject newCharacter;
    private Animator newCharacterAnimator;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip clickSound;
    public AudioClip[] randomSounds;
    public AudioSource audioWalkCorrect1;
    public AudioSource audioWalkCorrect2;
    public AudioSource audioWalkInCorrect1;
    public AudioSource audioWalkInCorrect2;
    public AudioSource audioWalkInCorrect3;

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        isMoving = false;
        animator.SetBool("isWalking", false);

        newCharacterAnimator = newCharacter.GetComponent<Animator>();
        newCharacter.SetActive(false);

        foreach (Button boton in botones)
        {
            if (boton != null)
            {
                boton.interactable = false;
            }
        }
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
                if (hit.collider.CompareTag("Square"))
                {
                    PlayToAudioClicIncorrect();
                    Debug.Log("Por ah� no puede caminar.");
                    return;
                }

                if (visited.Contains(hit.collider.tag))
                {
                    PlayToAudioClicIncorrect();
                    Debug.Log("Ya has pasado por aqu�.");
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
            if (hasCompletedFirstPath)
            {
                Debug.Log("Ya has completado el primer camino");
                return;
            }
            targetPosition = DestinationCorrecto;
            currentPath = "caminoCorrecto";
            Debug.Log("Dirigi�ndose a camino correcto");
            isMoving = true;

            if (botones.Count > 1 && botones[0] != null)
            {
                botones[0].interactable = true;
            }
            PlayTheAudioCorrect(audioWalkCorrect1);
        }
        else if (hit.CompareTag("caminoIncorrecto"))
        {
            if (hasCompletedFirstPath)
            {
                Debug.Log("Ya has completado el primer camino");
                return;
            }
            targetPosition = DestinationInorrecto;
            currentPath = "caminoIncorrecto";
            Debug.Log("Dirigi�ndose a camino incorrecto");
            isMoving = true;
            PlayTheAudioCorrect(audioWalkInCorrect1);
        }
        else if (hit.CompareTag("empezandoCaminar"))
        {
            if (hasCompletedFirstPath)
            {
                Debug.Log("Ya empezaste a caminar");
                return;
            }
            targetPosition = DestinationEmpezando;
            Debug.Log("Empiece a elegir el camino correcto");
            isMoving = true;
        }
        else if (hit.CompareTag("segundoCaminoCorrecto"))
        {
            if (!hasCompletedFirstPath)
            {
                Debug.Log("Debes completar el primer camino antes de avanzar");
                return;
            }
            if (hasCompletedSecondPath)
            {
                Debug.Log("Ya completaste este camino.");
                return;
            }
            if (waypoints.Count > 0)
            {
                currentWaypointIndex = 0;
                targetPosition = waypoints[currentWaypointIndex];
                currentPath = "segundoCaminoCorrecto";
                Debug.Log("Dirigi�ndose a lugar a salvo");
                isMoving = true;
            }
            if (botones.Count > 1 && botones[1] != null)
            {
                botones[1].interactable = true;
            }
            PlayTheAudioCorrect(audioWalkCorrect1);
        }
        else if (hit.CompareTag("segundoCaminoIncorrecto"))
        {
            if (!hasCompletedFirstPath || hasCompletedSecondPath)
            {
                Debug.Log("No puedes ir a este camino por ahora");
                return;
            }
            targetPosition = DestinationIncorrectoSegundo;
            currentPath = "segundoCaminoIncorrecto";
            Debug.Log("Est� pensando pasar por el puente");
            isMoving = true;
            PlayTheAudioCorrect(audioWalkInCorrect2);
        }
        else if (hit.CompareTag("Salir"))
        {
            if (!hasCompletedSecondPath)
            {
                Debug.Log("Debes completar el segundo camino antes de salir");
                return;
            }
            StartCoroutine(HandleCharacterSwitch());

            if (botones.Count > 1 && botones[2] != null)
            {
                botones[2].interactable = true;
            }
            PlayTheAudioCorrect(audioWalkCorrect2);
        }
        else if (hit.CompareTag("tercerCaminoIncorrecto"))
        {
            if (!hasCompletedSecondPath)
            {
                Debug.Log("No puedes ir a este camino por ahora");
                return;
            }
            targetPosition = DestinationIncorrectoTercer;
            currentPath = "tercerCaminoIncorrecto";
            Debug.Log("Camino Incorrecto");
            isMoving = true;
            PlayTheAudioCorrect(audioWalkInCorrect3);
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
            else if (targetPosition == DestinationIncorrectoSegundo)
            {
                targetPosition = DestinationCorrecto;
                Debug.Log("Regresando al punto de inicio...");
            }
            else if (targetPosition == DestinationIncorrectoTercer)
            {
                targetPosition = DestinationEmpezarTercer;
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
                if (currentPath == "caminoCorrecto")
                {
                    hasCompletedFirstPath = true;
                    visited.Add("caminoCorrecto");
                    visited.Add("caminoIncorrecto");
                }
                else if (currentPath == "segundoCaminoCorrecto")
                {
                    hasCompletedSecondPath = true;
                    visited.Add("segundoCaminoCorrecto");
                    visited.Add("segundoCaminoIncorrecto");
                }
            }
        }
        else
        {
            animator.SetBool("isWalking", true);
        }
    }

    private IEnumerator HandleCharacterSwitch()
    {
        // Fade out the current character
        SpriteRenderer currentRenderer = GetComponent<SpriteRenderer>();
        currentRenderer.color = new Color(currentRenderer.color.r, currentRenderer.color.g, currentRenderer.color.b, 0f);

        // Activate the new character
        newCharacter.SetActive(true);


        // Wait for the animation to finish
        yield return new WaitForSeconds(newCharacterAnimator.GetCurrentAnimatorStateInfo(0).length);

        if (GameManagerOsiPersonaje.Instance != null)
        {
            GameManagerOsiPersonaje.Instance.IncrementScore();
        }

        Debug.Log("Terminado");

        SceneManager.LoadSceneAsync("recompensas_juego2");
    }

    private void ShowDestinationMessage(Vector2 position)
    {
        if (position == DestinationCorrecto)
        {
            Debug.Log("Cruzaste sin problemas avanza al siguiente");

            if (GameManagerOsiPersonaje.Instance != null)
            {
                GameManagerOsiPersonaje.Instance.IncrementScore();
            }
        }
        else if (currentWaypointIndex == waypoints.Count - 1)
        {
            Debug.Log("Avanzaste muy bien");

            if (GameManagerOsiPersonaje.Instance != null)
            {
                GameManagerOsiPersonaje.Instance.IncrementScore();
            }
        }
    }

    void UpdateDirection(Vector2 newPosition)
    {
        Vector2 direction = (newPosition - rb.position).normalized;

        // Use a threshold to determine when we're "close enough" to cardinal directions
        /* float threshold = 0.7f; // You can adjust this value between 0.5 and 0.9

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
        } */
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            // Horizontal movement is dominant
            animator.SetFloat("Horizontal", Mathf.Sign(direction.x));
            animator.SetFloat("Vertical", 0f);
        }
        else if (Mathf.Abs(direction.y) > Mathf.Abs(direction.x))
        {
            // Vertical movement is dominant
            animator.SetFloat("Horizontal", 0f);
            animator.SetFloat("Vertical", Mathf.Sign(direction.y));
        }
        else
        {
            // Diagonal movement (equal x and y)
            // Prioritize one direction (e.g., horizontal) to avoid flickering
            animator.SetFloat("Horizontal", Mathf.Sign(direction.x));
            animator.SetFloat("Vertical", 0f);
        }

        // Ensure the walking animation is triggered
        animator.SetBool("isWalking", true);
    }
}
