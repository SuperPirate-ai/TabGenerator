using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using System.Net.Sockets;
using System.Collections.Concurrent;
using System.Text;
using System;

public class TCPClient : MonoBehaviour
{
    public static TCPClient Instance { get; private set; }
    private TcpClient client;
    private static readonly ConcurrentQueue<string> _messageQueue = new ConcurrentQueue<string>();
    bool running = false;
    // Start is called before the first frame update
    void Start()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        running = true;
        _ = Task.Run(StartTCPClient);
    }
    private void OnDestroy()
    {
        running = false;
        client?.Close();
    }
    private async Task StartTCPClient()
    {
        try
        {
            client = new TcpClient("127.0.0.1",9000);
            NetworkStream stream = client.GetStream();
            Debug.Log("Connected to server");

            byte[] recevingBuffer = new byte[1024];
            while (running)
            {
                while (_messageQueue.TryDequeue(out string message))
                {
                    byte[] data = Encoding.UTF8.GetBytes(message);
                    await stream.WriteAsync(data, 0, data.Length);
                }
                
                if (stream.DataAvailable)
                {
                    int bytesRead = await stream.ReadAsync(recevingBuffer, 0, recevingBuffer.Length);
                    string response = Encoding.UTF8.GetString(recevingBuffer, 0, bytesRead);
                    Console.WriteLine("Received: " + response);
                    EventManager.TriggerEvent("TCPMessage", new Dictionary<string, object> { { "message", response } });
                }

            }
            await Task.Delay(10);
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }
    public void SendData(string message)
    {
        _messageQueue.Enqueue(message);
    }
}
