using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using UnityEngine;
using System.Collections.Generic;

public class RobotDiscovery : MonoBehaviour
{
    [Header("Estado del Robot")]
    public string robotIP = "";
    public bool isDiscovered = false;

    [Header("Configuración Escáner")]
    public int targetPort = 8765; // El puerto del WebSocket
    public int timeoutMs = 800;   // Cuánto esperar por cada IP

    public event Action<string> OnRobotDiscovered;

    private async void Start()
    {
        await ScanNetworkAsync();
    }

    // Truco infalible para obtener la IP real del adaptador de Wi-Fi, ignorando adaptadores virtuales (VMware/Docker)
    private string GetLocalIPAddress()
    {
        using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
        {
            try 
            {
                socket.Connect("8.8.8.8", 65530);
                IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                return endPoint.Address.ToString();
            } 
            catch 
            {
                return "192.168.0.1"; // Fallback
            }
        }
    }

    private async Task ScanNetworkAsync()
    {
        string localIp = GetLocalIPAddress();
        Debug.Log($"[RobotDiscovery] Iniciando escaneo. Mi IP local es: {localIp}");
        
        string[] ipParts = localIp.Split('.');
        if (ipParts.Length != 4) return;
        
        string baseIp = $"{ipParts[0]}.{ipParts[1]}.{ipParts[2]}.";
        List<Task> tasks = new List<Task>();

        // Escanear las 254 IPs de la subred simultáneamente
        for (int i = 1; i <= 254; i++)
        {
            if (isDiscovered) break;
            
            string targetIp = baseIp + i;
            tasks.Add(CheckPortAsync(targetIp));
        }

        await Task.WhenAll(tasks);
        
        if (!isDiscovered)
        {
            Debug.LogWarning("[RobotDiscovery] Escaneo finalizado. No se encontró ningún robot en la red (o el Firewall bloqueó la conexión TCP).");
        }
    }

    private async Task CheckPortAsync(string ip)
    {
        try
        {
            using (TcpClient client = new TcpClient())
            {
                var connectTask = client.ConnectAsync(ip, targetPort);
                var timeoutTask = Task.Delay(timeoutMs);

                var completedTask = await Task.WhenAny(connectTask, timeoutTask);

                if (completedTask == connectTask && client.Connected)
                {
                    if (!isDiscovered) 
                    {
                        isDiscovered = true;
                        robotIP = ip;
                        Debug.Log($"[RobotDiscovery] ¡Bingo! Robot OSI encontrado en la IP: {robotIP}");
                        OnRobotDiscovered?.Invoke(robotIP);
                    }
                }
            }
        }
        catch
        {
            // Si el puerto está cerrado o no existe la IP, falla en silencio
        }
    }
}
