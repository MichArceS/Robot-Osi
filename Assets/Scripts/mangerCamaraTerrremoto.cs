using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MangerCamaraTerremoto : MonoBehaviour
{
    public Vector2 targetPosition; // La posición a la que se moverá la cámara
    public float speed = 2f; // Velocidad de movimiento

    void Start()
    {
        StartCoroutine(MoveCamera());
    }

    private IEnumerator MoveCamera()
    {
        while (true)
        {
            // Mueve la cámara hacia la posición objetivo
            transform.position = Vector3.MoveTowards(transform.position, new Vector3(targetPosition.x, targetPosition.y, transform.position.z), speed * Time.deltaTime);

            // Si llega a la posición objetivo, termina la coroutine
            if (Vector3.Distance(transform.position, new Vector3(targetPosition.x, targetPosition.y, transform.position.z)) < 0.1f)
            {
                Debug.Log("Reached target position.");
                yield break; // Termina la coroutine
            }

            yield return null; // Espera un frame
        }
    }
}
