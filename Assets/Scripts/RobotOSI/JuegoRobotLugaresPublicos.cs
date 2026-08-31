using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class JuegoRobotLugaresPublicos : MonoBehaviour
{
    [Header("Paneles")]
    [Header("Cliente WebSocket")]
    public RobotEmotionClient emotionClient;

    public GameObject panelOsiHablando;
    public GameObject panelOsiEsperando;
    public GameObject panelOsiEscuchando;
    public GameObject panelConfirmacion;

    [Header("Audios")]
    public AudioSource audioSource;
    public AudioClip audio1;  // Bienvenida
    public AudioClip audio2;
    public AudioClip audio3;
    public AudioClip audio4;  // Confirmaci�n
    public AudioClip audio5;  // Intro pregunta (si dice s�)
    public AudioClip audio6;  // Respuesta negativa
    public AudioClip audio7;  // Repetici�n por inactividad
    public AudioClip audio8;  // Despedida final
    public AudioClip audio9;
    public AudioClip audio10;
    public AudioClip audio12;
    public AudioClip audio13;
    public AudioClip audio14;
    public AudioClip audio15;
    public AudioClip audio16;
    public AudioClip audio18;  // Audio PostPregunta
    public AudioClip audio19;
    public AudioClip audio20;
    public AudioClip audio22;
    public AudioClip audio23;
    public AudioClip audio25;
    public AudioClip audio26;
    public AudioClip audio28;
    public AudioClip audio29;
    public AudioClip audio31;
    public AudioClip audio32;
    public AudioClip audio33;
    public AudioClip audio34;
    public AudioClip audio35;
    public AudioClip audio36;
    public AudioClip audio37;
    public AudioClip audio38;
    public AudioClip audio39;
    public AudioClip audio40;
    public AudioClip audio41;
    public AudioClip audio42;

    [Header("Audio de preguntas")] // Seccion para audios de preguntas
    public AudioClip audio17; // Hospital
    public AudioClip audio21; // Escuela
    public AudioClip audio24; // Restaurante
    public AudioClip audio27; // Mercado
    public AudioClip audio30; // Parque

    [Header("Configuraci�n")]
    public int indiceEscenaPreguntas = 1;
    public int indiceEscenaAlternativa = 0;
    public float tiempoOsiEsperando = 20f;
    public float tiempoInactividad = 10f;

    private int contadorNo = 0;
    private int repeticionesPorInactividad = 0;
    private Coroutine rutinaInactividad;
    private AudioClip ultimoAudioReproducido;
    public GrabadorDeRespuesta grabadorDeRespuesta;
    private int intentosSinHablar = 0;
    private int erroresConsecutivos = 0;
    public static bool desdeConfirmacion = false;

    private Dictionary<string, bool> progresoPreguntas = new Dictionary<string, bool>()
    {
        { "hospital", false },
        { "escuela", false },
        { "restaurante", false },
        { "mercado", false },
        { "parque", false }
    };
    public string preguntaActual = "";

    private void Start()
    {
        OcultarTodos();
        StartCoroutine(SecuenciaBienvenida());
    }

    private void OcultarTodos()
    {
        panelOsiHablando.SetActive(false);
        panelOsiEsperando.SetActive(false);
        panelOsiEscuchando.SetActive(false);
        panelConfirmacion.SetActive(false);
    }

        private string ultimaExpresion = "";
    private float tiempoUltimaExpresion = 0f;

    private IEnumerator ReproducirAudio(AudioClip clip)
    {
        if (clip == null)
        {
            Debug.LogWarning("AudioClip no asignado.");
            yield break;
        }

        ultimoAudioReproducido = clip; // Aqu� se guarda el clip actual
        audioSource.clip = clip;
        audioSource.Play();

        string animacionUsar = "hablar"; 
        if (Time.time - tiempoUltimaExpresion < 0.5f)
        {
            animacionUsar = ultimaExpresion;
        }
        else
        {
            StartCoroutine(EnviarExpresion("hablar"));
            animacionUsar = "hablar"; 
        }

        float tiempoRestante = clip.length;
        
        while (tiempoRestante > 0)
        {
            float tiempoEspera = Mathf.Min(6f, tiempoRestante);
            yield return new WaitForSeconds(tiempoEspera);
            tiempoRestante -= tiempoEspera;

            if (tiempoRestante > 0 && Time.time - tiempoUltimaExpresion >= 5.9f)
            {
                StartCoroutine(EnviarExpresion(animacionUsar));
            }
        }
    }

    public void RepetirAudioActual()
    {
        if (ultimoAudioReproducido != null)
        {
            audioSource.Stop();
            audioSource.clip = ultimoAudioReproducido;
            audioSource.Play();
            Debug.Log("Reproduciendo nuevamente: " + ultimoAudioReproducido.name);
        }
        else
        {
            Debug.LogWarning("No hay audio reproducido a�n para repetir.");
        }
    }

    private IEnumerator SecuenciaBienvenida()
    {
        panelOsiHablando.SetActive(true);

        yield return StartCoroutine(EnviarExpresion("alegre_hablando", true));
        yield return ReproducirAudio(audio1);

        yield return StartCoroutine(EnviarExpresion("curioso_hablando", true));
        yield return ReproducirAudio(audio2);

        yield return StartCoroutine(EnviarExpresion("neutral_hablando", true));
        yield return ReproducirAudio(audio3);

        yield return StartCoroutine(EnviarExpresion("neutral_reposo", true));
        MostrarConfirmacion();
    }

    private void MostrarConfirmacion(bool reproducirAudio = true)
    {
        OcultarTodos();
        panelConfirmacion.SetActive(true);

        if (reproducirAudio)
        {
            if (!progresoPreguntas["hospital"]) {
                audioSource.clip = audio4;
                audioSource.Play();
            }
        }

        if (rutinaInactividad != null)
            StopCoroutine(rutinaInactividad);
        rutinaInactividad = StartCoroutine(EsperarPorInactividad());
    }

    private IEnumerator EsperarPorInactividad()
    {
        OcultarTodos();
        panelConfirmacion.SetActive(true);

        yield return new WaitForSeconds(tiempoInactividad);

        repeticionesPorInactividad++;
        Debug.Log("Inactividad detectada (" + repeticionesPorInactividad + "/3)");

        if (repeticionesPorInactividad >= 3)
        {
            yield return ReproducirAudio(audio8);
            SceneManager.LoadScene(indiceEscenaAlternativa);
        }
        else
        {
            yield return ReproducirAudio(audio7);
            MostrarConfirmacion(reproducirAudio: false);
        }
    }

    public void OnClickConfirmarSi()
    {
        repeticionesPorInactividad = 0;

        if (rutinaInactividad != null)
            StopCoroutine(rutinaInactividad);

        JuegoRobotLugaresPublicos.desdeConfirmacion = true;
        StartCoroutine(ProcederAPreguntas());
    }
    public void OnClickConfirmarNo()
    {
        contadorNo++;
        Debug.Log("NO detectada (" + contadorNo + "/3)");

        if (rutinaInactividad != null)
            StopCoroutine(rutinaInactividad);

        if (contadorNo >= 3)
        {
            StartCoroutine(DespedidaYSalir());
            return;
        }

        StartCoroutine(MostrarOsiEsperandoYReintentar());
    }

    private IEnumerator MostrarOsiEsperandoYReintentar()
    {
        OcultarTodos();

        // 1. Reproduce audio6.mp3 (respuesta negativa)
        panelOsiHablando.SetActive(true);
        yield return ReproducirAudio(audio6);

        // 2. Mostrar OsiEsperando por 20 segundos
        panelOsiHablando.SetActive(false);
        panelOsiEsperando.SetActive(true);
        yield return new WaitForSeconds(tiempoOsiEsperando);

        // 3. Reproducir audio4 manualmente y luego mostrar confirmaci�n
        MostrarConfirmacion();
    }

    private IEnumerator ProcederAPreguntas()
    {
        if (desdeConfirmacion)
        {
            desdeConfirmacion = false;
            yield return ReproducirAudio(audio5);
        }

        if (!progresoPreguntas["hospital"])
        {
            preguntaActual = "hospital";
            yield return StartCoroutine(RealizarPreguntaHospital());
        }
        else if (!progresoPreguntas["escuela"])
        {
            preguntaActual = "escuela";
            yield return StartCoroutine(RealizarPreguntaEscuela());
        }
        else if (!progresoPreguntas["restaurante"])
        {
            preguntaActual = "restaurante";
            yield return StartCoroutine(RealizarPreguntaRestaurante());
        }
        else if (!progresoPreguntas["mercado"])
        {
            preguntaActual = "mercado";
            yield return StartCoroutine(RealizarPreguntaMercado());
        }
        else if (!progresoPreguntas["parque"])
        {
            preguntaActual = "parque";
            yield return StartCoroutine(RealizarPreguntaParque());
        }
        else
        {
            Debug.Log("�Todas las preguntas completadas!");
            yield return ReproducirAudio(audio8);  // Audio de despedida
            SceneManager.LoadScene(indiceEscenaAlternativa);
        }
    }

    // -----------------------------------------------------------------------------------------------------------------
    private IEnumerator RealizarPreguntaHospital()
    {
        OcultarTodos();
        panelOsiHablando.SetActive(true);

        // Pregunta y post-pregunta
        StartCoroutine(EnviarExpresion("enfermo_hablando"));
        yield return ReproducirAudio(audio17); // pregunta
        StartCoroutine(EnviarExpresion("enfermo_hablando"));
        yield return ReproducirAudio(audio18); // dilo fuerte

        panelOsiHablando.SetActive(false);
        panelOsiEscuchando.SetActive(true);

        Debug.Log("Osi hizo la pregunta. Inicia grabaci�n de voz.");

        grabadorDeRespuesta.EsperarYGrabarCuandoHable(this);
        yield return new WaitForSeconds(60f); // mismo tiempo de espera, pero ahora solo para detecci�n

        if (!grabadorDeRespuesta.HabloEnUltimaGrabacion())
        {
            intentosSinHablar++;
            Debug.LogWarning("Usuario no habl�. Intento " + intentosSinHablar);

            if (intentosSinHablar >= 3)
            {
                panelOsiEscuchando.SetActive(false);
                panelOsiHablando.SetActive(true);
                yield return ReproducirAudio(audio8);
                SceneManager.LoadScene(indiceEscenaAlternativa);
            }
            else
            {
                yield return StartCoroutine(RealizarPreguntaHospital());  // Repetir pregunta sin intro
            }
        }
        else
        {
            intentosSinHablar = 0;
            Debug.Log("Usuario habl�, continuar el flujo normal...");
            // Contin�a el flujo normal del juego aqu�
        }
    }

    private IEnumerator RealizarPreguntaEscuela()
    {
        preguntaActual = "escuela";
        OcultarTodos();
        panelOsiHablando.SetActive(true);

        // Pregunta y post-pregunta
        StartCoroutine(EnviarExpresion("curioso_hablando"));
        yield return ReproducirAudio(audio21);
        StartCoroutine(EnviarExpresion("curioso_hablando"));
        yield return ReproducirAudio(audio18);

        panelOsiHablando.SetActive(false);
        panelOsiEscuchando.SetActive(true);

        Debug.Log("Osi hizo la pregunta. Inicia grabaci�n de voz.");

        grabadorDeRespuesta.EsperarYGrabarCuandoHable(this);
        yield return new WaitForSeconds(60f); // mismo tiempo de espera, pero ahora solo para detecci�n

        if (!grabadorDeRespuesta.HabloEnUltimaGrabacion())
        {
            intentosSinHablar++;
            Debug.LogWarning("Usuario no habl�. Intento " + intentosSinHablar);

            if (intentosSinHablar >= 3)
            {
                panelOsiEscuchando.SetActive(false);
                panelOsiHablando.SetActive(true);
                yield return ReproducirAudio(audio8);
                SceneManager.LoadScene(indiceEscenaAlternativa);
            }
            else
            {
                yield return StartCoroutine(RealizarPreguntaEscuela());  // Repetir pregunta sin intro
            }
        }
        else
        {
            intentosSinHablar = 0;
            Debug.Log("Usuario habl�, continuar el flujo normal...");
            // Contin�a el flujo normal del juego aqu�
        }
    }

    private IEnumerator RealizarPreguntaRestaurante()
    {
        preguntaActual = "restaurante";
        OcultarTodos();
        panelOsiHablando.SetActive(true);

        // Pregunta y post-pregunta
        StartCoroutine(EnviarExpresion("hambriento_hablando"));
        yield return ReproducirAudio(audio24);
        StartCoroutine(EnviarExpresion("hambriento_hablando"));
        yield return ReproducirAudio(audio18);

        panelOsiHablando.SetActive(false);
        panelOsiEscuchando.SetActive(true);

        Debug.Log("Osi hizo la pregunta. Inicia grabaci�n de voz.");

        grabadorDeRespuesta.EsperarYGrabarCuandoHable(this);
        yield return new WaitForSeconds(60f); // mismo tiempo de espera, pero ahora solo para detecci�n

        if (!grabadorDeRespuesta.HabloEnUltimaGrabacion())
        {
            intentosSinHablar++;
            Debug.LogWarning("Usuario no habl�. Intento " + intentosSinHablar);

            if (intentosSinHablar >= 3)
            {
                panelOsiEscuchando.SetActive(false);
                panelOsiHablando.SetActive(true);
                yield return ReproducirAudio(audio8);
                SceneManager.LoadScene(indiceEscenaAlternativa);
            }
            else
            {
                yield return StartCoroutine(RealizarPreguntaRestaurante());  // Repetir pregunta sin intro
            }
        }
        else
        {
            intentosSinHablar = 0;
            Debug.Log("Usuario habl�, continuar el flujo normal...");
            // Contin�a el flujo normal del juego aqu�
        }
    }

    private IEnumerator RealizarPreguntaMercado()
    {
        preguntaActual = "mercado";
        OcultarTodos();
        panelOsiHablando.SetActive(true);

        // Pregunta y post-pregunta
        StartCoroutine(EnviarExpresion("curioso_hablando"));
        yield return ReproducirAudio(audio27);
        StartCoroutine(EnviarExpresion("curioso_hablando"));
        yield return ReproducirAudio(audio18);

        panelOsiHablando.SetActive(false);
        panelOsiEscuchando.SetActive(true);

        Debug.Log("Osi hizo la pregunta. Inicia grabaci�n de voz.");

        grabadorDeRespuesta.EsperarYGrabarCuandoHable(this);
        yield return new WaitForSeconds(60f); // mismo tiempo de espera, pero ahora solo para detecci�n

        if (!grabadorDeRespuesta.HabloEnUltimaGrabacion())
        {
            intentosSinHablar++;
            Debug.LogWarning("Usuario no habl�. Intento " + intentosSinHablar);

            if (intentosSinHablar >= 3)
            {
                panelOsiEscuchando.SetActive(false);
                panelOsiHablando.SetActive(true);
                yield return ReproducirAudio(audio8);
                SceneManager.LoadScene(indiceEscenaAlternativa);
            }
            else
            {
                yield return StartCoroutine(RealizarPreguntaMercado());  // Repetir pregunta sin intro
            }
        }
        else
        {
            intentosSinHablar = 0;
            Debug.Log("Usuario habl�, continuar el flujo normal...");
            // Contin�a el flujo normal del juego aqu�
        }
    }

    private IEnumerator RealizarPreguntaParque()
    {
        preguntaActual = "parque";
        OcultarTodos();
        panelOsiHablando.SetActive(true);

        // Pregunta y post-pregunta
        StartCoroutine(EnviarExpresion("curioso_hablando"));
        yield return ReproducirAudio(audio30);
        StartCoroutine(EnviarExpresion("curioso_hablando"));
        yield return ReproducirAudio(audio18);

        panelOsiHablando.SetActive(false);
        panelOsiEscuchando.SetActive(true);

        Debug.Log("Osi hizo la pregunta. Inicia grabaci�n de voz.");

        grabadorDeRespuesta.EsperarYGrabarCuandoHable(this);
        yield return new WaitForSeconds(60f); // mismo tiempo de espera, pero ahora solo para detecci�n

        if (!grabadorDeRespuesta.HabloEnUltimaGrabacion())
        {
            intentosSinHablar++;
            Debug.LogWarning("Usuario no habl�. Intento " + intentosSinHablar);

            if (intentosSinHablar >= 3)
            {
                panelOsiEscuchando.SetActive(false);
                panelOsiHablando.SetActive(true);
                yield return ReproducirAudio(audio8);
                SceneManager.LoadScene(indiceEscenaAlternativa);
            }
            else
            {
                yield return StartCoroutine(RealizarPreguntaParque());  // Repetir pregunta sin intro
            }
        }
        else
        {
            intentosSinHablar = 0;
            Debug.Log("Usuario habl�, continuar el flujo normal...");
            // Contin�a el flujo normal del juego aqu�
        }
    }

    // ------------------------------------------------------------------------------------------------------------------

    public bool EvaluarRespuesta(RespuestaAPI respuestaAPI, string s)
    {
        if (respuestaAPI.predicted_text.Contains(s))
        {
            return true;
        }
        return false;
    }

    public IEnumerator ProcesarResultadoDesdeAPI(RespuestaAPI respuesta)
    {
        if (respuesta == null)
        {
            Debug.LogError("La respuesta de la API es nula.");
            yield break;
        }

        OcultarTodos();
        panelOsiHablando.SetActive(true);

        bool continuar = false;

        if (respuesta.success)
        {
            if (EvaluarRespuesta(respuesta, "hospital") && preguntaActual == "hospital") continuar = true;
            else if (EvaluarRespuesta(respuesta, "escuela") && preguntaActual == "escuela") continuar = true;
            else if (EvaluarRespuesta(respuesta, "restaurante") && preguntaActual == "restaurante") continuar = true;
            else if (EvaluarRespuesta(respuesta, "mercado") && preguntaActual == "mercado") continuar = true;
            else if (EvaluarRespuesta(respuesta, "parque") && preguntaActual == "parque") continuar = true;
        }


        if (continuar)
        {
            StartCoroutine(EnviarExpresion("alegre"));
            erroresConsecutivos = 0;
            progresoPreguntas[preguntaActual] = true; // Marca como respondida

            Debug.Log("Respuesta correcta: " + respuesta.predicted_text);
            

            if (preguntaActual.Contains("hospital"))
            {
                StartCoroutine(EnviarExpresion("alegre_hablando"));
                yield return ReproducirAudio(audio40);
                yield return ReproducirAudio(audio19);
                yield return ReproducirAudio(audio9);
                MostrarConfirmacion();
                yield return ReproducirAudio(audio13);
            }
            else if (preguntaActual.Contains("escuela"))
            {
                StartCoroutine(EnviarExpresion("alegre_hablando"));
                yield return ReproducirAudio(audio39);
                yield return ReproducirAudio(audio22);
                yield return ReproducirAudio(audio12);
                MostrarConfirmacion();
                yield return ReproducirAudio(audio13);
            }
            else if (preguntaActual.Contains("restaurante"))
            {
                StartCoroutine(EnviarExpresion("alegre_hablando"));
                yield return ReproducirAudio(audio38);
                yield return ReproducirAudio(audio25);
                yield return ReproducirAudio(audio9);
                yield return ReproducirAudio(audio14);
                MostrarConfirmacion();
                yield return ReproducirAudio(audio13);
            }
            else if (preguntaActual.Contains("mercado"))
            {
                StartCoroutine(EnviarExpresion("alegre_hablando"));
                yield return ReproducirAudio(audio40);
                yield return ReproducirAudio(audio29);
                yield return ReproducirAudio(audio16);
                MostrarConfirmacion();
                yield return ReproducirAudio(audio13);
            }
            else if (preguntaActual.Contains("parque"))
            {
                StartCoroutine(EnviarExpresion("alegre_hablando"));
                yield return ReproducirAudio(audio42);
                yield return ReproducirAudio(audio31);
                yield return StartCoroutine(DespedidaYSalir());
            }
        }
        else
        {
            erroresConsecutivos++;
            Debug.Log("Respuesta incorrecta (" + erroresConsecutivos + "): " + respuesta.predicted_text);

            // Retroalimentaci�n por intento
            AudioClip audioError = erroresConsecutivos switch
            {
                1 => audio37,
                2 => audio35,
                3 => audio36,
                _ => null
            };

            if (audioError != null)
                StartCoroutine(EnviarExpresion("sorprendido_hablando"));
                yield return ReproducirAudio(audioError);

            if (erroresConsecutivos >= 3)
            {
                erroresConsecutivos = 0;
                if (preguntaActual.Contains("hospital"))
                {
                    progresoPreguntas[preguntaActual] = true; // Se marca aunque fall�
                    yield return ReproducirAudio(audio20);
                    yield return StartCoroutine(ProcederAPreguntas());
                }
                else if (preguntaActual.Contains("escuela"))
                {
                    progresoPreguntas[preguntaActual] = true; 
                    yield return ReproducirAudio(audio23);
                    yield return StartCoroutine(ProcederAPreguntas());
                }
                else if (preguntaActual.Contains("restaurante"))
                {
                    progresoPreguntas[preguntaActual] = true; 
                    yield return ReproducirAudio(audio26);
                    yield return StartCoroutine(ProcederAPreguntas());
                }
                else if (preguntaActual.Contains("mercado"))
                {
                    progresoPreguntas[preguntaActual] = true; 
                    yield return ReproducirAudio(audio28);
                    yield return StartCoroutine(ProcederAPreguntas());
                }
                else if (preguntaActual.Contains("parque"))
                {
                    progresoPreguntas[preguntaActual] = true; 
                    yield return ReproducirAudio(audio32);
                    yield return StartCoroutine(DespedidaYSalir());
                }
            }
            else
            {
                // Repite solo la pregunta actual
                yield return StartCoroutine(RepetirPreguntaActual());
            }
        }
    }

    private IEnumerator RepetirPreguntaActual()
    {
        switch (preguntaActual)
        {
            case "hospital":
                yield return StartCoroutine(RealizarPreguntaHospital());
                break;
            case "escuela":
                yield return StartCoroutine(RealizarPreguntaEscuela());
                break;
            case "restaurante":
                yield return StartCoroutine(RealizarPreguntaRestaurante());
                break;
            case "mercado":
                yield return StartCoroutine(RealizarPreguntaMercado());
                break;
            case "parque":
                yield return StartCoroutine(RealizarPreguntaParque());
                break;
        }
    }


    private IEnumerator DespedidaYSalir()
    {
        OcultarTodos();
        panelOsiHablando.SetActive(true);
        yield return ReproducirAudio(audio33);
        yield return ReproducirAudio(audio34);
        StartCoroutine(EnviarExpresion("sorprendido_hablando"));
        StartCoroutine(EnviarExpresion("alegre_hablando"));
        StartCoroutine(EnviarExpresion("alegre_hablando"));
        SceneManager.LoadScene(indiceEscenaAlternativa);
    }

        public IEnumerator EnviarExpresion(string expresion)
    {
        string expresionLimpia = expresion.Replace("_hablando", "").ToLower();
        
        ultimaExpresion = expresionLimpia;
        tiempoUltimaExpresion = Time.time;

        if (emotionClient != null)
        {
            emotionClient.SendEmotion(expresionLimpia);
        }
        else
        {
            Debug.LogWarning("No se ha asignado emotionClient en JuegoRobotLugaresPublicos.");
        }
        
        yield return null;
    }

    public IEnumerator EnviarExpresion(string expresion, bool test)
    {
        yield return new WaitForSeconds(1f);
    }
}

[System.Serializable]
public class AccionData
{
    public string accion;
}

[System.Serializable]
public class RespuestaAPI
{
    public string status;
    public bool success;
    public string predicted_text;
    public string created_at;
}
