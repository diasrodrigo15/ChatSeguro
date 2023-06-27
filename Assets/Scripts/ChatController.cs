using System;
using System.Net.Sockets;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatController : MonoBehaviour
{
    [SerializeField] private TMP_InputField _key;
    [SerializeField] private TMP_InputField _ipAddress;
    [SerializeField] private TMP_InputField _messageInput;
    [SerializeField] private Toggle _rc4Toggle;
    [SerializeField] private Toggle _sdesToggle;

    [SerializeField] private Button _sendButton;

    private RC4 _rc4;
    private byte[] _keyBytes;

    private void Awake()
    {
        _sendButton.onClick.AddListener(SendEncryptedMessage);
    }
    
    private void SetupCyphers()
    {
        _keyBytes = System.Text.Encoding.UTF8.GetBytes(_key.text);
        _rc4 = new RC4(_keyBytes);
    }

    private void SendEncryptedMessage()
    {
        SetupCyphers();
        string message = _messageInput.text;

        byte[] encryptedMessage;
        if (_rc4Toggle.isOn) // RC4
        {
            byte[] messageBytes = System.Text.Encoding.UTF8.GetBytes(message);
            encryptedMessage = _rc4.Encrypt(messageBytes);
        }
        else // S-DES
        {
            encryptedMessage = SDES.Encrypt(message, _key.text);
        }

        // Enviar a mensagem cifrada para o endereço IP de destino
        SendDataInNetwork(encryptedMessage);

        _messageInput.text = string.Empty;
    }
    
    private void SendDataInNetwork(byte[] data)
    {
        try
        {
            int targetPort = 3000;
            
            // Create a TCP client
            TcpClient client = new TcpClient();

            // Connect to the server
            client.Connect(_ipAddress.text, targetPort);

            // Get the network stream
            NetworkStream stream = client.GetStream();

            // Send the message
            stream.Write(data, 0, data.Length);

            // Close the stream and client
            stream.Close();
            client.Close();

            Debug.Log("Message sent successfully!");
        }
        catch (Exception e)
        {
            Debug.LogError("Error sending message: " + e.Message);
        }
    }

    public string DecryptMessage(byte[] encryptedMessage)
    {
        if (_rc4Toggle.isOn) // RC4
        {
            SetupCyphers();
            return _rc4.Decrypt(encryptedMessage);
        }
        else // S-DES
        {
            return SDES.Decrypt(encryptedMessage, _key.text);
        }
    }
}
