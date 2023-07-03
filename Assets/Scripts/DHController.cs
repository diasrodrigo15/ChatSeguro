using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DHController : MonoBehaviour
{
    public event Action OnDhUsageChanged;
    
    private const int DhListenPort = 3030;

    [SerializeField] private TextMeshProUGUI _ipAddress;
    [SerializeField] private TextMeshProUGUI _errorMessage;
    [SerializeField] private Toggle _dhUsageToggle;
    [SerializeField] private Button _generateSessionButton;
    [SerializeField] private TextMeshProUGUI _sessionKeyLabel;

    public bool IsDhReady => !_dhUsageToggle.isOn || _sessionKey != null;
    public byte[] SessionKey => _sessionKey;

    private DHKeyExchange _dhKeyExchange;
    private string _otherPublicKey;
    private byte[] _sessionKey = null;

    private TcpListener _listener;
    private bool _isListening = false;
    private bool _sendingMyKey;

    private void Awake()
    {
        _dhKeyExchange = new DHKeyExchange();
        _dhKeyExchange.GeneratePublicKey();

        UpdateDhUsage();
        _generateSessionButton.onClick.AddListener(GenerateSessionKey);
        _dhUsageToggle.onValueChanged.AddListener(HandleDhToggleChanged);

        StartListening();
    }

    private void HandleDhToggleChanged(bool isOn)
    {
        UpdateDhUsage();
    }

    private void OnDisable()
    {
        StopListening();
    }

    private void UpdateDhUsage()
    {
        bool useDh = _dhUsageToggle.isOn;

        _generateSessionButton.interactable = useDh;
        _sessionKeyLabel.enabled = useDh;
        if (_sessionKey != null)
        {
            _sessionKeyLabel.text = Encoding.UTF8.GetString(_sessionKey);
        }
        
        OnDhUsageChanged?.Invoke();
    }
    
    private byte[] StringToByteArray(string str)
    {
        return Encoding.UTF8.GetBytes(str);
    }

    private async void GenerateSessionKey()
    {
        if (_sendingMyKey)
        {
            return;
        }

        _sendingMyKey = true;
        SendPublicKey(StringToByteArray(_dhKeyExchange.GetPublicKey()));

        while (string.IsNullOrEmpty(_otherPublicKey))
        {
            await Task.Yield();
        }

        _dhKeyExchange.CalculateSharedKey(_otherPublicKey);
        _sendingMyKey = false;
        
        _sessionKey = _dhKeyExchange.DeriveSessionKey();
        OnDhUsageChanged?.Invoke();
    }

    private void SendPublicKey(byte[] data)
    {
        try
        {
            // Create a TCP client
            TcpClient client = new TcpClient();

            // Connect to the server
            client.Connect(_ipAddress.text, DhListenPort);

            // Get the network stream
            NetworkStream stream = client.GetStream();

            // Send the message
            stream.Write(data, 0, data.Length);

            // Close the stream and client
            stream.Close();
            client.Close();

            _errorMessage.enabled = false;
        }
        catch (Exception e)
        {
            _errorMessage.enabled = true;
            _errorMessage.text = "Couldn't exchange keys: " + e.Message;
        }
    }

    #region Receive Public Key

    private void StartListening()
    {
        try
        {
            // Create a TCP listener
            _listener = new TcpListener(IPAddress.Any, DhListenPort);

            // Start listening for incoming connections
            _listener.Start();
            _isListening = true;

            // Begin accepting client connections asynchronously
            _listener.BeginAcceptTcpClient(OnClientConnected, null);

            Debug.Log("Started listening for incoming messages...");
        }
        catch (Exception e)
        {
            Debug.LogError("Error starting listener: " + e.Message);
        }
    }

    private void StopListening()
    {
        if (_isListening)
        {
            // Stop listening for incoming connections
            _listener.Stop();
            _isListening = false;

            Debug.Log("Stopped listening for incoming messages.");
        }
    }

    private void OnClientConnected(IAsyncResult ar)
    {
        try
        {
            // Accept the client connection
            TcpClient client = _listener.EndAcceptTcpClient(ar);

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
            } while (stream.DataAvailable);

            // Process the received message
            _otherPublicKey = messageBuilder.ToString();
            if (!_sendingMyKey)
            {
                GenerateSessionKey();
            }

            // Close the stream and client
            stream.Close();
            client.Close();

            // Continue listening for more connections
            _listener.BeginAcceptTcpClient(OnClientConnected, null);
        }
        catch (Exception e)
        {
            Debug.LogError("Error handling client connection: " + e.Message);
        }
    }

    #endregion
}
