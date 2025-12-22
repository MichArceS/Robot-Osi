using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfiniteBackground : MonoBehaviour
{
    public float scrollSpeed = 0.5f; // Velocidad de desplazamiento del fondo
    private Renderer _renderer;

    void Start()
    {
        _renderer = GetComponent<Renderer>();
    }

    void Update()
    {
        // Calcula el desplazamiento de la textura basado en el tiempo
        float offset = Time.time * scrollSpeed;
        _renderer.material.mainTextureOffset = new Vector2(offset, 0);
    }
}
