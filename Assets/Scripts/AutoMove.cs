using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cinemachine;
using UnityEngine.EventSystems;

public class AutoMove : MonoBehaviour
{
    private float moveSpeed = 5f;
    public List<GameObject> validTargets;
    private Vector2 targetPosition;
    private bool isMoving = false;
    private Animator animator;
    public Animator tvAnimator;
    private Rigidbody2D rb;
    private bool firstClick = true;
    private int clickCount = 0;

    public Vector2 mochilaDestination;
    public Vector2 tvDestination;
    public List<Button> botones;

    public CinemachineVirtualCamera camera1;
    public CinemachineVirtualCamera camera2;

    [Header("Objetos a Desactivar")]
    [SerializeField] private GameObject objeto1;
    [SerializeField] private GameObject objeto2;


    [Header("Objetos a Activar")]
    [SerializeField] private GameObject PersonajeNuevo;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip clickSound;
    public AudioClip[] randomSounds;
    public AudioSource audioTv;
    public AudioSource audioMochila;

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        isMoving = false;

        camera1.gameObject.SetActive(true);
        camera2.gameObject.SetActive(false);

        // Desactivar todos los botones al iniciar
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
            animator.SetBool("IsMovingOsi", true);
            firstClick = false;
        }

        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 clickedPosition = new Vector2(mousePos.x, mousePos.y + 2f);
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

            if (tvAnimator != null && tvAnimator.GetBool("isMovingTv"))
            {
                tvAnimator.SetBool("isMovingTv", false);
                //Debug.Log("Animaci�n de la TV desactivada.");
            }

            if (hit)
            {
                Debug.Log(hit);
                Debug.Log(hit.collider.gameObject.name);
                if (validTargets.Contains(hit.collider.gameObject))
                {
                    if (hit.collider.gameObject.CompareTag("Mochila"))
                    {
                        targetPosition = mochilaDestination;
                        //Debug.Log("Dirigi�ndose a la mochila");
                    }
                    else if (hit.collider.gameObject.CompareTag("tv"))
                    {
                        targetPosition = tvDestination;
                        //Debug.Log("Dirigi�ndose a la televisi�n");
                    }
                    else if (hit.collider.gameObject.CompareTag("Piso"))
                    {
                        targetPosition = clickedPosition;
                        //Debug.Log("Lugar correcto");
                    }

                    isMoving = true;
                }
                else
                {
                    PlayToAudioClicIncorrect();
                    isMoving = false;
                    //Debug.Log("Lugar incorrecto");
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
            ShowCollisionMessage();
        }
    }

    private void ShowCollisionMessage()
    {
        Debug.Log("Te has chocado con una pared");
    }

    private void ShowDestinationMessage(Vector2 position)
    {
        if (position == mochilaDestination)
        {
            PlayTheAudioCorrect(audioMochila);

            if (botones.Count > 1 && botones[0] != null)
            {
                botones[0].interactable = true;
            }

            StartCoroutine(WaitAndPrint(ReactivateNewCharacter));

            bool isCamera1Active = camera1.gameObject.activeSelf;
            camera1.gameObject.SetActive(!isCamera1Active);
            camera2.gameObject.SetActive(isCamera1Active);

            if (GameManagerOsiPersonaje.Instance != null)
            {
                GameManagerOsiPersonaje.Instance.IncrementScore();
            }
        }
        else if (position == tvDestination)
        {
            Debug.Log("Llegaste a la televisi�n, �mira la televisi�n!");
            PlayTheAudioCorrect(audioTv);
            if (botones.Count > 1 && botones[1] != null)
            {
                botones[1].interactable = true;
            }
            StartCoroutine(WaitAndPrint(StartTVAnimation));

            if (GameManagerOsiPersonaje.Instance != null)
            {
                GameManagerOsiPersonaje.Instance.IncrementScore();
            }
        }
    }

    private IEnumerator WaitAndPrint(System.Action action)
    {
        yield return new WaitForSeconds(0.5f);
        action?.Invoke();
    }

    private void ReactivateNewCharacter()
    {

        if (objeto1 != null && objeto2 != null)
        {
            objeto1.SetActive(false);
            objeto2.SetActive(false);
        }

        if (PersonajeNuevo != null)
        {
            PersonajeNuevo.SetActive(true);
        }
    }

    private void StartTVAnimation()
    {
        GameObject tvObject = GameObject.FindGameObjectWithTag("tv");
        if (tvObject != null)
        {
            tvAnimator = tvObject.GetComponent<Animator>();

            if (tvAnimator != null)
            {
                tvAnimator.SetBool("isMovingTv", true);

                Debug.Log("Animaci�n de la TV iniciada.");
            }
        }
        else
        {
            Debug.LogWarning("No se encontr� el objeto de la TV.");
        }
    }
}
