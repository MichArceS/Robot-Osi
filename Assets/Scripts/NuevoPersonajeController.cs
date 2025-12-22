using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;


public class NuevoPersonajeController : MonoBehaviour
{
    private Rigidbody2D rb2D;
    private Animator animator;

    private float movimientoHorizontal;
    private float movimientoVertical;
    private bool nuevoIniciado = false;

    [SerializeField] private float velocidadDeMovimiento;
    [SerializeField] private Vector2 direccion;

    [Header("Camaras")]
    public CinemachineVirtualCamera camera1;
    public CinemachineVirtualCamera camera2;

    private void Start()
    {
        rb2D = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        camera1.gameObject.SetActive(true);
        camera2.gameObject.SetActive(false);
    }

    private void Update()
    {
        // Captura la entrada del jugador
        movimientoHorizontal = Input.GetAxisRaw("Horizontal");
        movimientoVertical = Input.GetAxisRaw("Vertical");

        animator.SetFloat("MovimientoOsiY", movimientoVertical);
        animator.SetFloat("MovimientoOsiX", movimientoHorizontal);

        // Iniciar movimiento solo si hay entrada
        if (!nuevoIniciado && (movimientoHorizontal != 0 || movimientoVertical != 0))
        {
            nuevoIniciado = true; 
            animator.SetBool("NuevoComienzo", true);
        }

        if (movimientoHorizontal != 0 || movimientoVertical != 0)
        {
            animator.SetFloat("UltimoOsiY", movimientoVertical);
            animator.SetFloat("UltimoOsiX", movimientoHorizontal);
            
        }

        direccion = new Vector2(movimientoHorizontal, movimientoVertical).normalized;

        
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Salir"))
        {
            Debug.Log("¡Saliendo, continúa!");

            camera1.gameObject.SetActive(false);
            camera2.gameObject.SetActive(true);

        }
    }

    private void FixedUpdate()
    {
        // Mueve el personaje
        rb2D.MovePosition(rb2D.position + direccion * velocidadDeMovimiento * Time.deltaTime);
    }
}
