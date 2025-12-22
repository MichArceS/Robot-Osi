using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cinemachine;
using UnityEngine.EventSystems;

public class controllerOsiErupcion : MonoBehaviour
{
    [SerializeField] private float speed;
    private Rigidbody2D rb;
    private Animator animator;

    private bool isMoving = false;
    private int currentWaypointIndex = 0;
    private List<Vector2> waypoints = new List<Vector2>();
    public List<Button> botones;

    private enum Destination
    {
        Ninguno,
        Mascarilla,
        Mochila,
    }

    private Destination currentDestination = Destination.Ninguno;

    public Vector2[] waypointsMascarilla;
    public Vector2[] waypointsMochila;

    private Vector2 targetPosition;
    private bool firstClick = true;
    private int clickCount = 0;

    [Header("Objetos a Activar")]
    [SerializeField] private GameObject osiPrincipal;
    [SerializeField] private GameObject mascarillaOsi;
    //[SerializeField] private GameObject osiMochila;
    [SerializeField] private GameObject Mascarilla;
    [SerializeField] private CinemachineVirtualCamera camara1;
    [SerializeField] private CinemachineVirtualCamera camara2;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip clickSound;
    public AudioClip[] randomSounds;
    public AudioSource audioMascarilla;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

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
        if (Input.GetMouseButtonDown(0))
        {
            if (firstClick)
            {
                animator.SetBool("isMoving", true);
                firstClick = false;
            }
            HandleInput();
        }

        if (isMoving)
        {
            MoveTowardsTarget();
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
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


    private void HandleInput()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 clickedPosition = new Vector2(mousePos.x, mousePos.y + 2f);
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

        if (hit.collider != null)
        {
            HandleRaycastHit(hit.collider.tag, clickedPosition);
        }
        else
        {
            SetTargetPosition(clickedPosition);
        }
    }

    private void HandleRaycastHit(string tag, Vector2 clickedPosition)
    {
        switch (tag)
        {
            case "Pared":
                PlayToAudioClicIncorrect();
                break;
            case "Mascarilla":
                SetWaypoints(waypointsMascarilla, Destination.Mascarilla, "Dirigi�ndose a la mascarilla");
                break;
            case "Mochila":
                SetWaypoints(waypointsMochila, Destination.Mochila, "Dirigi�ndome a recoger la mochila");
                break;
            default:
                SetTargetPosition(clickedPosition);
                break;
        }
    }

    private void SetWaypoints(Vector2[] newWaypoints, Destination destination, string message)
    {
        waypoints = new List<Vector2>(newWaypoints);
        currentWaypointIndex = 0;
        currentDestination = destination;
        isMoving = true;
        Debug.Log(message);
    }

    private void SetTargetPosition(Vector2 position)
    {
        targetPosition = position;
        isMoving = true;
        currentWaypointIndex = -1;
        currentDestination = Destination.Ninguno;
    }

    private void MoveTowardsTarget()
    {
        if (currentWaypointIndex >= 0 && waypoints.Count > 0)
        {
            MoveThroughWaypoints();
        }
        else if (currentWaypointIndex == -1)
        {
            MoveToTargetPosition();
        }
    }

    private void MoveThroughWaypoints()
    {
        if (currentWaypointIndex < waypoints.Count)
        {
            Vector2 targetWaypoint = waypoints[currentWaypointIndex];
            MoveToPosition(targetWaypoint);

            if (Vector2.Distance(rb.position, targetWaypoint) < 0.1f)
            {
                currentWaypointIndex++;
                if (currentWaypointIndex >= waypoints.Count)
                {
                    EndMovement();
                }
            }
        }
    }

    private void MoveToTargetPosition()
    {
        MoveToPosition(targetPosition);

        if (Vector2.Distance(rb.position, targetPosition) < 0.1f)
        {
            EndMovement();
            Debug.Log("�Llegaste a la posici�n clickeada!");
        }
    }

    private void MoveToPosition(Vector2 position)
    {
        Vector2 newPosition = Vector2.MoveTowards(rb.position, position, speed * Time.deltaTime);
        rb.MovePosition(newPosition);
        UpdateDirection(newPosition);
        animator.SetBool("isWalking", true);
    }

    private void EndMovement()
    {
        isMoving = false;
        animator.SetBool("isWalking", false);
        HandleArrivalMessage();
    }

    private void HandleArrivalMessage()
    {
        switch (currentDestination)
        {
            case Destination.Mascarilla:
                PlayTheAudioCorrect(audioMascarilla);
                StartCoroutine(WaitOsi(ReactiveToCharter));
                break;
            case Destination.Mochila:
                Debug.Log("�Llegaste a recoger la mochila! Vuelva primero recoga la mascarilla.");
                break;
            default:
                break;
        }
        // Restablecer estado
        currentDestination = Destination.Ninguno;
        waypoints.Clear();
    }

    private IEnumerator WaitOsi(System.Action action)
    {
        yield return new WaitForSeconds(0.5f);
        action?.Invoke();
    }

    private void ReactiveToCharter()
    {
        if (mascarillaOsi != null)
        {
            mascarillaOsi.SetActive(true);
            osiPrincipal.SetActive(false);
            Mascarilla.SetActive(false);
        }
        camara1.gameObject.SetActive(false);
        camara2.gameObject.SetActive(true);
        if (botones.Count > 0 && botones[0] != null)
        {
            botones[0].interactable = true;
        }
    }


    private void UpdateDirection(Vector2 newPosition)
    {
        Vector2 direction = (newPosition - rb.position).normalized;

        // Use a threshold to determine when we're "close enough" to cardinal directions
        float threshold = 0.7f; // You can adjust this value between 0.5 and 0.9

        if (Mathf.Abs(direction.x) > threshold)
        {
            // Pure horizontal movement
            animator.SetFloat("MovimientoX", Mathf.Sign(direction.x));
            animator.SetFloat("MovimientoY", 0f);
        }
        else if (Mathf.Abs(direction.y) > threshold)
        {
            // Pure vertical movement
            animator.SetFloat("MovimientoX", 0f);
            animator.SetFloat("MovimientoY", Mathf.Sign(direction.y));
        }
        else
        {
            // For diagonal movement, choose based on which direction is larger
            if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
            {
                animator.SetFloat("MovimientoX", Mathf.Sign(direction.x));
                animator.SetFloat("MovimientoY", 0f);
            }
            else
            {
                animator.SetFloat("MovimientoX", 0f);
                animator.SetFloat("MovimientoY", Mathf.Sign(direction.y));
            }
        }
    }
}
