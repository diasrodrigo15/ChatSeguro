using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

public class Receiver : MonoBehaviour
{
    [SerializeField] private int listenPort = 3000;
    [SerializeField] private ChatController _chatController;

    private TcpListener listener;
    private bool isListening = false;

    private void Start()
    {
        StartListening();
    }

    private void OnDisable()
    {
        StopListening();
    }

    private void StartListening()
    {
        try
        {
            // Create a TCP listener
            listener = new TcpListener(IPAddress.Any, listenPort);

            // Start listening for incoming connections
            listener.Start();
            isListening = true;

            // Begin accepting client connections asynchronously
            listener.BeginAcceptTcpClient(OnClientConnected, null);

            Debug.Log("Started listening for incoming messages...");
        }
        catch (Exception e)
        {
            Debug.LogError("Error starting listener: " + e.Message);
        }
    }

    private void StopListening()
    {
        if (isListening)
        {
            // Stop listening for incoming connections
            listener.Stop();
            isListening = false;

            Debug.Log("Stopped listening for incoming messages.");
        }
    }

    private void OnClientConnected(IAsyncResult ar)
    {
        try
        {
            // Accept the client connection
            TcpClient client = listener.EndAcceptTcpClient(ar);

            // Start reading the incoming message asynchronously
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[1024];
            StringBuilder messageBuilder = new StringBuilder();

            // Read the incoming message
            int bytesRead;
            do
            {
                bytesRead = stream.Read(buffer, 0, buffer.Length);
                messageBuilder.Append(Encoding.UTF8.GetString(buffer, 0, bytesRead));
            }
            while (stream.DataAvailable);

            // Process the received message
            string receivedMessage = messageBuilder.ToString();
            byte[] encryptedMessage = Encoding.UTF8.GetBytes(receivedMessage);
            string decryptedMessage = _chatController.DecryptMessage(encryptedMessage);
            
            Debug.Log("Received message: " + decryptedMessage);

            // Close the stream and client
            stream.Close();
            client.Close();

            // Continue listening for more connections
            listener.BeginAcceptTcpClient(OnClientConnected, null);
        }
        catch (Exception e)
        {
            Debug.LogError("Error handling client connection: " + e.Message);
        }
    }

}
