using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DHController : MonoBehaviour
{
    public event Action OnDhUsageChanged;
    
    private const int DhListenPort = 3000;

    [SerializeField] private Toggle _dhUsageToggle;
    [SerializeField] private Button _generateSessionButton;
    [SerializeField] private TextMeshProUGUI _sessionKeyLabel;

    public bool IsDhReady => !_dhUsageToggle.isOn || _sessionKey != null;
    public byte[] SessionKey => _sessionKey;

    private DHKeyExchange _dhKeyExchange;
    private Sender _sender;
    private ChatController _chatController;
    private string _otherPublicKey;
    private byte[] _sessionKey = null;

    private bool _sendingMyKey;

    private void Awake()
    {
        _dhKeyExchange = new DHKeyExchange();
        _dhKeyExchange.GeneratePublicKey();

        UpdateDhUsage();
        _generateSessionButton.onClick.AddListener(GenerateSessionKey);
        _dhUsageToggle.onValueChanged.AddListener(HandleDhToggleChanged);

        _sender = FindObjectOfType<Sender>();
        _chatController = FindObjectOfType<ChatController>();
    }

    private void HandleDhToggleChanged(bool isOn)
    {
        if (!isOn)
        {
            _sendingMyKey = false;
            _sessionKey = null;
            _otherPublicKey = string.Empty;
        }
 
        UpdateDhUsage();
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

    private void GenerateSessionKey()
    {
        if (_sendingMyKey)
        {
            return;
        }
 
        _sessionKeyLabel.enabled = true;
        _sessionKeyLabel.text = "Gerando chave...";
        _sendingMyKey = true;

        _sender.OnFinishedSent += HandleFinishSent;
        byte[] encryptedMessage = _chatController.GetEncryptedMessage($"DH_{_dhKeyExchange.GetPublicKey()}");
        _sender.SendDataInNetwork(encryptedMessage);
    }

    private async void HandleFinishSent(bool success)
    {
        _sender.OnFinishedSent -= HandleFinishSent;
        if (!success)
        {
            _sendingMyKey = false;
            _sessionKeyLabel.enabled = false;
            return;
        }

        _sessionKeyLabel.text = "Aguardando chave do par...";
        while (string.IsNullOrEmpty(_otherPublicKey))
        {
            await Task.Yield();
        }

        _dhKeyExchange.CalculateSharedKey(_otherPublicKey);
        _sendingMyKey = false;

        _sessionKey = _dhKeyExchange.DeriveSessionKey();
        _sessionKeyLabel.text = $"Chave: {Encoding.UTF8.GetString(_sessionKey)}";

        OnDhUsageChanged?.Invoke();
    }


    public void SetOtherPublicKey(string otherKey)
    {
        _otherPublicKey = otherKey;
        if (!_sendingMyKey)
        {
            GenerateSessionKey();
        }
    }
}
