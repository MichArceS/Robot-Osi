using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class RobotEmotionClient : MonoBehaviour
{
    private ClientWebSocket ws;
    private Uri serverUri;
    private CancellationTokenSource cts;

    [Header("Autodescubrimiento")]
    [Tooltip("Arrastra aquí el objeto que tiene el script RobotDiscovery")]
    public RobotDiscovery robotDiscovery;

    // Se conecta cuando el robot es descubierto
    private void Start()
    {
        if (robotDiscovery != null)
        {
            if (robotDiscovery.isDiscovered)
            {
                // Si el robot ya fue descubierto antes de que inicie este script, nos conectamos directamente
                ConnectToDiscoveredRobot(robotDiscovery.robotIP);
            }
            else
            {
                // Si aún no ha sido descubierto, nos suscribimos al evento para que nos avise
                robotDiscovery.OnRobotDiscovered += ConnectToDiscoveredRobot;
            }
        }
        else
        {
            Debug.LogWarning("[RobotEmotionClient] No se ha asignado el RobotDiscovery en el inspector. El WebSocket no sabrá a qué IP conectarse.");
        }
    }

    private async void ConnectToDiscoveredRobot(string ip)
    {
        serverUri = new Uri($"ws://{ip}:8765");
        await ConnectToServer();
    }

    private async Task ConnectToServer()
    {
        ws = new ClientWebSocket();
        cts = new CancellationTokenSource();

        try
        {
            await ws.ConnectAsync(serverUri, cts.Token);
            Debug.Log("Conectado al servidor de WebSocket del Robot OSI.");
            
            // Si alguien intentó enviar una emoción mientras buscábamos la IP, envíala ahora.
            if (!string.IsNullOrEmpty(pendingEmotion))
            {
                string emotionToSend = pendingEmotion;
                pendingEmotion = null;
                SendEmotion(emotionToSend);
            }
            
            // Iniciar tarea para escuchar mensajes
            _ = ReceiveMessages();
        }
        catch (Exception e)
        {
            Debug.LogError($"Error conectando al WebSocket: {e.Message}");
        }
    }

    private async Task ReceiveMessages()
    {
        var buffer = new byte[1024];
        while (ws != null && ws.State == WebSocketState.Open)
        {
            try
            {
                var result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), cts.Token);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Cerrando", cts.Token);
                }
            }
            catch (Exception)
            {
                break;
            }
        }
    }

    private string pendingEmotion = null;

    /// <summary>
    /// Envía una emoción al servidor del robot.
    /// </summary>
    /// <param name="emotionName">El nombre de la emoción.</param>
    public async void SendEmotion(string emotionName)
    {
        emotionName = emotionName.ToLower();

        if (ws == null || ws.State != WebSocketState.Open)
        {
            if (serverUri != null)
            {
                Debug.LogWarning("El WebSocket no está conectado. Intentando reconectar...");
                await ConnectToServer();
            }
            else
            {
                Debug.LogWarning($"Aún no se ha descubierto la IP del robot. Guardando emoción '{emotionName}' en la cola de espera...");
                pendingEmotion = emotionName;
                return;
            }
        }

        if (ws != null && ws.State == WebSocketState.Open)
        {
            // Validar la emoción (Opcional, pero previene envíos incorrectos)
            string[] validEmotions = { "alegre", "curioso", "enfermo", "hambriento", "sorprendido", "neutral" };
            if (Array.IndexOf(validEmotions, emotionName) == -1)
            {
                Debug.LogWarning($"La emoción '{emotionName}' no está en la lista de valores esperados. Se enviará de todos modos.");
            }

            // Construir el JSON como string.
            string jsonMessage = $"{{\"emotion\": \"{emotionName}\"}}";
            byte[] bytesToSend = Encoding.UTF8.GetBytes(jsonMessage);

            try
            {
                await ws.SendAsync(new ArraySegment<byte>(bytesToSend), WebSocketMessageType.Text, true, cts.Token);
                Debug.Log($"[RobotEmotionClient] Emoción enviada exitosamente: {jsonMessage}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Error al enviar la emoción: {e.Message}");
            }
        }
    }

    private async void OnDestroy()
    {
        if (ws != null && ws.State == WebSocketState.Open)
        {
            cts?.Cancel();
            await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Cerrando aplicación", CancellationToken.None);
        }
        ws?.Dispose();
        cts?.Dispose();
    }
}

