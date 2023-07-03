using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

public class Receiver : MonoBehaviour
{
    [SerializeField] private int listenPort = 3000;
    [SerializeField] private ChatController _chatController;
    [Space]
    [SerializeField] private MessageContentGroup _messagesContent;

    private TcpListener listener;
    private bool isListening = false;

    private DHController _dhController;

    private void Start()
    {
        _dhController = FindObjectOfType<DHController>();
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
                byte[] data = new byte[bytesRead];
                for (int i = 0; i < bytesRead; i++)
                {
                    data[i] = buffer[i];
                }
                
                string decryptedBlock = _chatController.DecryptMessage(data);
                messageBuilder.Append(decryptedBlock);
            }
            while (stream.DataAvailable);

            if (messageBuilder.ToString().Contains("DH_"))
            {
                // update dh key
                _dhController.SetOtherPublicKey(messageBuilder.ToString().Remove(0,3)); // remove DH_ from string
                _messagesContent.AddMessage($"Recebido chave public do par", MessageType.Received);
            }
            else
            {
                // Print message in history
                _messagesContent.AddMessage(messageBuilder.ToString(), MessageType.Received);
            }

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
