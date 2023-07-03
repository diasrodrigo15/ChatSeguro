using System;
using System.Net.Sockets;
using TMPro;
using UnityEngine;

public class Sender : MonoBehaviour
{
    public event Action<bool> OnFinishedSent;
    public int TargetPort = 3000;

    [SerializeField] private TMP_InputField _ipAddress;
    [SerializeField] private TextMeshProUGUI _errorMessage;
    
    public void SendDataInNetwork(byte[] data)
    {
        try
        {
            // Create a TCP client
            TcpClient client = new TcpClient();

            // Connect to the server
            client.Connect(_ipAddress.text, TargetPort);

            // Get the network stream
            NetworkStream stream = client.GetStream();

            // Send the message
            stream.Write(data, 0, data.Length);

            // Close the stream and client
            stream.Close();
            client.Close();

            _errorMessage.enabled = false;
            OnFinishedSent?.Invoke(true);
        }
        catch (Exception e)
        {
            _errorMessage.enabled = true;
            _errorMessage.text = "Error sending message: " + e.Message;
            OnFinishedSent?.Invoke(false);
        }
    }
}