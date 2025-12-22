using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MochilaCompletaScript : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private GameObject tv;
    [SerializeField] private GameObject tvPanel1;
    private Rigidbody2D rb;
    private Animator animator;

    private bool isMoving = false;
    private int currentWaypointIndex = 0;
    private List<Vector2> waypoints = new List<Vector2>();
    public List<Button> botones;

    private enum Destination
    {
        Ninguno,
        Alimentos,
        Tv,
        Puerta
    }

    private Destination currentDestination = Destination.Ninguno;

    public Vector2[] waypointsAlimentos;
    public Vector2[] waypointsTv;
    public Vector2[] waypointsPuerta;


    private Vector2 targetPosition;
    private SpriteRenderer tvSpriteRenderer;
    private SpriteRenderer osiSpriteRenderer;
    private int clickCount = 0;

    [Header("Personajes")]
    [SerializeField] private GameObject OsiPrincipal;
    [SerializeField] private GameObject osiMochilaAtras;
    [SerializeField] private GameObject osiMochilaAtras1;
    [SerializeField] private GameObject osiMochilaUltimo;

    [Header("Desactivar Activar Objetos")]
    [SerializeField] private GameObject tapa1;
    [SerializeField] private GameObject tapa2;

    [Header("Nuevo Destino")]
    [SerializeField] private Vector2 nuevoDestino;


    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip clickSound;
    public AudioClip[] randomSounds;
    public AudioSource audioAlimentos;
    public AudioSource audioTv;


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        //tvSpriteRenderer = tv.GetComponent<SpriteRenderer>();
        osiSpriteRenderer = OsiPrincipal.GetComponent<SpriteRenderer>();

        StartCoroutine(WaitBeforeMoveNew());
    }

    private IEnumerator WaitBeforeMoveNew()
    {
        yield return new WaitForSeconds(0.5f);
        SetTargetPosition(nuevoDestino);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            HandleInputNew();
        }

        if (isMoving)
        {
            MoveTowardsTargetNew();
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


    private void HandleInputNew()
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
            case "Alimentos":
                SetWaypoints(waypointsAlimentos, Destination.Alimentos, "Dirigi�ndose a tapar los alimentos");
                break;
            case "tv":
                SetWaypoints(waypointsTv, Destination.Tv, "Dirigi�ndome a la televisi�n");
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

    private void MoveTowardsTargetNew()
    {
        if (currentWaypointIndex >= 0 && waypoints.Count > 0)
        {
            MoveThroughWaypointsNew();
        }
        else if (currentWaypointIndex == -1)
        {
            MoveToTargetPositionNew();
        }
    }

    private void MoveThroughWaypointsNew()
    {
        if (currentWaypointIndex < waypoints.Count)
        {
            Vector2 targetWaypoint = waypoints[currentWaypointIndex];
            MoveToPositionNew(targetWaypoint);

            if (Vector2.Distance(rb.position, targetWaypoint) < 0.1f)
            {
                currentWaypointIndex++;
                if (currentWaypointIndex >= waypoints.Count)
                {
                    EndMovementNew();
                }
            }
        }
    }

    private void MoveToTargetPositionNew()
    {
        MoveToPositionNew(targetPosition);

        if (Vector2.Distance(rb.position, targetPosition) < 0.1f)
        {
            EndMovementNew();
            Debug.Log("�Llegaste a la posici�n clickeada!");
        }
    }

    private void MoveToPositionNew(Vector2 position)
    {
        Vector2 newPosition = Vector2.MoveTowards(rb.position, position, speed * Time.deltaTime);
        rb.MovePosition(newPosition);
        UpdateDirection(newPosition);
        animator.SetBool("isWalking", true);
    }

    private void EndMovementNew()
    {
        isMoving = false;
        animator.SetBool("isWalking", false);
        HandleArrivalMessageNew();
    }

    private void HandleArrivalMessageNew()
    {
        switch (currentDestination)
        {
            case Destination.Alimentos:
                HandleAlimentosArrival();
                break;
            case Destination.Tv:
                PlayTheAudioCorrect(audioTv);
                activateCharter(osiMochilaAtras1);
                HandleTvArrival();
                break;
            case Destination.Puerta:
                HandlePuertaArrival();
                break;
            default:
                break;
        }

        currentDestination = Destination.Ninguno;
        waypoints.Clear();
    }

    private void HandleAlimentosArrival()
    {
        activateCharter(osiMochilaAtras);
        StartCoroutine(WaitAnimationNew(cubridorAlimentos));
        StartCoroutine(WaitForOneSecond(reactivandoPersonajes));
        PlayTheAudioCorrect(audioAlimentos);
    }

    private void HandleTvArrival()
    {
        if (tvPanel1 != null)
        {
            tvPanel1.SetActive(true);
        }
        StartCoroutine(WaitAnimationNew(tvObjectAnimationNew));
        StartCoroutine(PlayTVAnimation());
        StartCoroutine(WaitForOneSecond(reactivandoPersonajes2));
        StartCoroutine(MoveToDoorAfterDelay());
    }

    private IEnumerator PlayTVAnimation()
    {
        Animator tvAnimator = tv.GetComponent<Animator>();
        AudioSource tvAudioSource = tv.GetComponent<AudioSource>();
        if (tvAudioSource == null)
        {
            Debug.Log("No se encontró el AudioSource en el objeto de la TV.");
        }
        if (tvAnimator != null)
        {
            // Trigger the TV animation
            tvAnimator.SetTrigger("PlayTV");

            // Wait for the animation to complete
            // We'll use a more reliable way to wait for the animation to finish
            yield return new WaitUntil(() =>
                tvAnimator.GetCurrentAnimatorStateInfo(0).IsName("TVAnimation1"));
            float animationLength = tvAnimator.GetCurrentAnimatorStateInfo(0).length;
            Debug.Log("Duración de la animación de la TV: " + animationLength);
            float timeToWait = 100f / 60;
            if (timeToWait > animationLength)
            {
                Debug.LogWarning("El tiempo de espera es mayor a la duración de la animación.");
                yield break;
            }
            yield return new WaitUntil(() =>
                tvAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= (timeToWait / animationLength));

            if (tvAudioSource != null)
            {
                tvAudioSource.Play();
                Debug.Log("Audio de la TV reproducido.");
                yield return new WaitWhile(() => tvAudioSource.isPlaying);
            }
        }
        else
        {
            Debug.LogWarning("No se encontró el Animator en el objeto de la TV.");
        }

        yield return new WaitForSeconds(8f); // Optional: Wait before deactivating the TV and moving to the door
    }

    private IEnumerator MoveToDoorAfterDelay()
    {
        yield return new WaitForSeconds(7f);
        if (tvPanel1 != null)
        {
            tvPanel1.SetActive(false);
        }
        else
        {
            Debug.LogWarning("No se encontró el objeto de la TV.");
        }
        SetWaypoints(waypointsPuerta, Destination.Puerta, "Dirigi�ndose a la puerta");
    }

    private void HandlePuertaArrival()
    {
        activateDesactivateObject(osiSpriteRenderer, 0f);
        osiMochilaUltimo.SetActive(true);
        StartCoroutine(WaitAndChangeScene());
    }

    private IEnumerator WaitAndChangeScene()
    {
        yield return new WaitForSeconds(1.5f);
        SceneManager.LoadScene("erupcion_Volcanica2");
    }

    private IEnumerator WaitForOneSecond(System.Action action)
    {
        yield return new WaitForSeconds(1.8f);
        action?.Invoke();
    }

    private IEnumerator WaitAnimationNew(System.Action action)
    {
        yield return new WaitForSeconds(0.5f);
        action?.Invoke();
    }

    private void reactivandoPersonajes()
    {
        if (osiMochilaAtras != null)
        {
            osiMochilaAtras.SetActive(false);
            activateDesactivateObject(osiSpriteRenderer, 1f);
        }
    }

    private IEnumerator WaitForTenSeconds()
    {
        yield return new WaitForSeconds(10f);
    }

    private void reactivandoPersonajes2()
    {
        StartCoroutine(WaitForTenSeconds());
        if (osiMochilaAtras1 != null)
        {
            osiMochilaAtras1.SetActive(false);
            activateDesactivateObject(osiSpriteRenderer, 1f);
        }
        //activateDesactivateObject(tvSpriteRenderer, 0f);
    }

    private void cubridorAlimentos()
    {
        if (botones.Count > 0 && botones[0] != null)
        {
            botones[0].interactable = true;
        }

        if (tapa1 != null && tapa2 != null)
        {
            tapa1.SetActive(false);
            tapa2.SetActive(true);
        }
    }

    private void tvObjectAnimationNew()
    {
        //activateDesactivateObject(tvSpriteRenderer, 1f);
        if (botones.Count > 0 && botones[1] != null)
        {
            botones[1].interactable = true;
        }
    }

    private void activateDesactivateObject(SpriteRenderer spriteRenderer, float alpha)
    {
        if (spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            color.a = alpha;
            spriteRenderer.color = color;
        }
    }

    private void activateCharter(GameObject osiMochila)
    {
        if (osiMochila != null)
        {
            osiMochila.SetActive(true);
            activateDesactivateObject(osiSpriteRenderer, 0f);
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
