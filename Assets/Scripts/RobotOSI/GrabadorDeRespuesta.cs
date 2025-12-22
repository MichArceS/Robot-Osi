using System;
using System.IO;
using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class GrabadorDeRespuesta : MonoBehaviour
{
    [Header("Datos del juego")]
    public string minijuego;
    public int intento = 1;

    [Header("Grabación")]
    public int duracionGrabacion = 30;
    public int frecuencia = 16000;

    [Header("Audio de la pregunta")]
    public AudioClip audioPregunta;
    private string nombrePregunta;

    private AudioClip clipGrabado;
    private string rutaAudio;
    private string rutaJSON;
    private string nombreArchivoAudio;
    private string codigoJugador;
    public GameObject imagenREC;

    public void IniciarGrabacion()
    {
        // Obtener código del jugador
        codigoJugador = PlayerPrefs.GetString("PlayerCodeContinue", "Desconocido");

        // Obtener nombre de la pregunta desde el audio
        nombrePregunta = audioPregunta != null ? audioPregunta.name + ".mp3" : "PreguntaDesconocida";

        minijuego = SceneManager.GetActiveScene().name;

        // Preparar nombres de archivo
        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        nombreArchivoAudio = $"{codigoJugador}_respuesta_{timestamp}.wav";
        rutaAudio = Path.Combine(Application.persistentDataPath, nombreArchivoAudio);
        rutaJSON = Path.Combine(Application.persistentDataPath, Path.GetFileNameWithoutExtension(nombreArchivoAudio) + ".json");

        if (imagenREC != null) imagenREC.SetActive(true);

        // Iniciar grabación
        Debug.Log("Iniciando grabación...");
        clipGrabado = Microphone.Start(null, false, duracionGrabacion, frecuencia);
        Invoke(nameof(DetenerYProcesar), duracionGrabacion);
    }

    private void DetenerYProcesar()
    {
        Debug.Log("Grabación terminada.");
        Microphone.End(null);

        if (imagenREC != null) imagenREC.SetActive(false);

        bool hablo = DetectarSiHablo(clipGrabado);
        habloUltimaVez = hablo;

        if (!hablo)
        {
            Debug.LogWarning("No se detectó voz. No se subirá el audio.");
            return;
        }

        GuardarWav(clipGrabado, rutaAudio);
        GuardarJSON(hablo);

        Debug.Log($"Audio guardado en: {rutaAudio}");
        Debug.Log($"JSON guardado en: {rutaJSON}");

        StartCoroutine(SubirAudioYProcesar(rutaAudio));
    }


    private bool DetectarSiHablo(AudioClip clip, float umbral = 0.01f)
    {
        float[] datos = new float[clip.samples];
        clip.GetData(datos, 0);

        foreach (float sample in datos)
        {
            if (Mathf.Abs(sample) > umbral)
                return true;
        }
        return false;
    }

    public void EsperarYGrabarCuandoHable(MonoBehaviour contexto)
    {
        contexto.StartCoroutine(EsperarYDetectar());
    }

    private IEnumerator EsperarYDetectar()
    {
        Debug.Log("Esperando hasta 1 minuto por detección de voz...");

        float tiempoMaximo = 60f;
        float tiempoTranscurrido = 0f;
        float[] buffer = new float[frecuencia];
        habloUltimaVez = false;

        while (tiempoTranscurrido < tiempoMaximo)
        {
            AudioClip clipTemp = Microphone.Start(null, false, 1, frecuencia);
            yield return new WaitForSeconds(1f);
            Microphone.End(null);

            clipTemp.GetData(buffer, 0);

            foreach (float sample in buffer)
            {
                if (Mathf.Abs(sample) > 0.01f)
                {
                    Debug.Log("Voz detectada. Iniciando grabación real.");
                    habloUltimaVez = true;
                    IniciarGrabacion();  //inicia grabacion de 30s
                    yield break;
                }
            }

            tiempoTranscurrido += 1f;
        }

        Debug.Log("No se detectó voz en 1 minuto.");
    }


    private IEnumerator SubirAudioYProcesar(string path)
    {
        Debug.Log("Subiendo audio a la API...");

        byte[] audioData = File.ReadAllBytes(path);

        WWWForm form = new WWWForm();
        form.AddBinaryData("audio", audioData, Path.GetFileName(path), "audio/wav");

        UnityWebRequest request = UnityWebRequest.Post("http://127.0.0.1:3658/m1/1010269-996503-default/evaluar-respuesta", form);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Subida exitosa: " + request.downloadHandler.text);

            string json = request.downloadHandler.text;
            RespuestaAPI respuesta = JsonUtility.FromJson<RespuestaAPI>(json);
            JuegoRobotLugaresPublicos juego = FindObjectOfType<JuegoRobotLugaresPublicos>();
            juego.StartCoroutine(juego.ProcesarResultadoDesdeAPI(respuesta));
        }
        else
        {
            Debug.LogError("Error al subir audio: " + request.error);
        }
    }

    private void GuardarJSON(bool hablo)
    {
        RespuestaUsuario respuesta = new RespuestaUsuario
        {
            jugador = codigoJugador,
            minijuego = minijuego,
            pregunta = nombrePregunta,
            archivoAudio = Path.GetFileName(nombreArchivoAudio),
            fecha = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            hablo = hablo,
            intento = intento
        };

        string json = JsonUtility.ToJson(respuesta, true);
        File.WriteAllText(rutaJSON, json);
    }

    private void GuardarWav(AudioClip clip, string ruta)
    {
        byte[] wavData = WavUtility.FromAudioClip(clip);
        File.WriteAllBytes(ruta, wavData);
    }

    private bool habloUltimaVez = false;

    public bool HabloEnUltimaGrabacion()
    {
        return habloUltimaVez;
    }

}

[Serializable]
public class RespuestaUsuario
{
    public string jugador;
    public string minijuego;
    public string pregunta;
    public string archivoAudio;
    public string fecha;
    public bool hablo;
    public int intento;
}
