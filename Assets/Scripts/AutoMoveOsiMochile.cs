using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AutoMoveOsiMochile : MonoBehaviour
{
    private float moveSpeed = 5f;
    public List<GameObject> validTargets;
    public List<Button> botones;
    public Vector2 tvDestination;
    public Vector2 puertaDestination;

    private Vector3 targetPosition;
    private bool isMoving;
    private Animator animator;
    private Rigidbody2D rb;
    private int clickCount = 0;

    [Header("Objetos a Desactivar")]
    [SerializeField] private GameObject PersonajePrincipal;

    [Header("Objetos a Activar")]
    [SerializeField] private GameObject PersonajeFin;

    [Header("Objeto de la TV")]
    [SerializeField] private GameObject tvObject;
    [SerializeField] private GameObject tvPanel;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip clickSound;
    public AudioClip[] randomSounds;
    public AudioSource audioTv;

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        isMoving = false;
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
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 clickedPosition = new Vector2(mousePos.x, mousePos.y + 2f);
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

            if (hit)
            {
                if (validTargets.Contains(hit.collider.gameObject))
                {
                    if (hit.collider.gameObject.CompareTag("tv"))
                    {
                        targetPosition = tvDestination;
                        Debug.Log("Dirigi�ndose a la televisi�n");
                    }
                    else if (hit.collider.gameObject.CompareTag("Puerta"))
                    {
                        targetPosition = puertaDestination;
                        Debug.Log("Dirigi�ndose a la Puerta");
                    }
                    else if (hit.collider.gameObject.CompareTag("Piso"))
                    {
                        targetPosition = clickedPosition;
                    }

                    isMoving = true;
                }
                else
                {
                    PlayToAudioClicIncorrect();
                    isMoving = false;
                }
            }
        }

        if (isMoving)
        {
            MoveTowardsTarget();
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

    void MoveTowardsTarget()
    {
        Vector2 newPosition = Vector2.MoveTowards(rb.position, targetPosition, moveSpeed * Time.deltaTime);
        rb.MovePosition(newPosition);

        UpdateDirection(newPosition);

        if (Vector2.Distance(rb.position, targetPosition) < 0.1f)
        {
            isMoving = false;
            animator.SetBool("isWalking", false);
            ShowDestinationMessage(targetPosition);
        }
        else
        {
            animator.SetBool("isWalking", true);
        }
    }

    void UpdateDirection(Vector2 newPosition)
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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Pared"))
        {
            isMoving = false;
            animator.SetBool("isWalking", false);
            ShowCollisionMessageOsi();
        }
    }

    private void ShowCollisionMessageOsi()
    {
        Debug.Log("Te has chocado con una pared");
    }

    private void ShowDestinationMessage(Vector3 position)
    {
        if (position == (Vector3)tvDestination)
        {
            PlayTheAudioCorrect(audioTv);
            if (botones.Count > 1 && botones[1] != null)
            {
                botones[1].interactable = true;
            }

            StartCoroutine(WaitAndPrint(ActivateTV));

            if (GameManagerOsiPersonaje.Instance != null)
            {
                GameManagerOsiPersonaje.Instance.IncrementScore();
            }
        }
        else if (position == (Vector3)puertaDestination)
        {
            Debug.Log("Llegaste a la Salida");
            StartCoroutine(WaitAndPrint(SaliendoWalking));
        }
    }

    private IEnumerator WaitAndPrint(System.Action action)
    {
        yield return new WaitForSeconds(0.5f);
        action?.Invoke();
    }

    private void ActivateTV()
    {
        if (tvObject != null)
        {
            tvObject.SetActive(true); // Activa la TV
            if (tvPanel != null)
            {
                tvPanel.SetActive(true); // Activa el panel de la TV
            }
            Debug.Log("TV activada.");
            StartCoroutine(PlayTVAnimation());
            // Esperar a que termine la animación de la TV
            StartCoroutine(DeactivateTVAndMoveToDoor());
        }
        else
        {
            Debug.LogWarning("No se encontr� el objeto de la TV.");
        }
    }

    private IEnumerator PlayTVAnimation()
    {
        Animator tvAnimator = tvObject.GetComponent<Animator>();
        AudioSource tvAudioSource = tvObject.GetComponent<AudioSource>();
        if (tvAnimator == null)
        {
            Debug.Log("No se encontró el Animator en el objeto de la TV.");
        }
        if (tvAudioSource == null)
        {
            Debug.Log("No se encontró el AudioSource en el objeto de la TV.");
        }

        tvAnimator.SetTrigger("PlayTV");
        yield return new WaitUntil(() => tvAnimator.GetCurrentAnimatorStateInfo(0).IsName("TVAnimation"));
        float animationLength = tvAnimator.GetCurrentAnimatorStateInfo(0).length;
        Debug.Log("Duración de la animación de la TV: " + animationLength);
        float timeToWait = 90f / 60;
        if (timeToWait > animationLength)
        {
            Debug.LogWarning("El tiempo de espera es mayor a la duración de la animación.");
            yield break;
        }
        yield return new WaitUntil(() => tvAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= (timeToWait / animationLength));

        if (tvAudioSource != null)
        {
            tvAudioSource.Play();
            Debug.Log("Audio de la TV reproducido.");
        }

        if (tvAudioSource != null)
        {
            yield return new WaitWhile(() => tvAudioSource.isPlaying);
        }
        yield return new WaitForSeconds(6f); // Optional: Wait before deactivating the TV and moving to the door
    }



    private IEnumerator DeactivateTVAndMoveToDoor()
    {
        // Espera 2 segundos
        yield return new WaitForSeconds(7f);

        // Desactivar la TV
        if (tvObject != null)
        {
            tvObject.SetActive(false);
            Debug.Log("TV desactivada.");
        }

        if (tvPanel != null)
        {
            tvPanel.SetActive(false);
            Debug.Log("Panel de la TV desactivado.");
        }

        // Dirigirse a la puerta
        targetPosition = puertaDestination;
        isMoving = true; // Comienza a moverse hacia la puerta
    }

    private void SaliendoWalking()
    {
        if (PersonajePrincipal != null)
        {
            PersonajePrincipal.SetActive(false);
        }
        if (PersonajeFin != null)
        {
            PersonajeFin.SetActive(true);
        }
        Debug.LogWarning("Fin");
    }
}
