using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class PulseSprite : MonoBehaviour
{
    private void Awake()
    {
        // Obtener el SpriteRenderer
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();

        // Configurar el color inicial (blanco semi-transparente)
        spriteRenderer.color = new Color(1f, 1f, 1f, 0.5f);

        // Configurar el modo de renderizado
        spriteRenderer.sortingOrder = 1;
        spriteRenderer.maskInteraction = SpriteMaskInteraction.None;

        // Configurar el material
        spriteRenderer.material.shader = Shader.Find("Sprites/Default");

        // Configurar la escala inicial
        transform.localScale = new Vector3(0.5f, 0.5f, 1f);
    }
}