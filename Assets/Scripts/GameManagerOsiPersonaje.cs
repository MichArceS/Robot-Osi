using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManagerOsiPersonaje : MonoBehaviour
{
    public static GameManagerOsiPersonaje Instance { get; private set; }

    public int score; // Contador que se mantendrá entre escenas

    private void Awake()
    {
        // Si ya existe otra instancia de GameManagerOsiPersonaje, destrúyela
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Mantiene este objeto entre escenas
        }
    }

    public void IncrementScore()
    {
        score++;
        Debug.Log("Puntuación actual: " + score);
    }
}
