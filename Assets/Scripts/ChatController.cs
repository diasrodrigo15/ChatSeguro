using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatController : MonoBehaviour
{
    private const int SDES_OPTION = 0;
    private const int RC4_OPTION = 1;
    private const int CBC_OPTION = 2;
    private const int EBC_OPTION = 3;
    
    [SerializeField] private TMP_InputField _key;
    [SerializeField] private TMP_InputField _messageInput;
    [SerializeField] private TMP_Dropdown _cypherMode;
    [SerializeField] private MessageContentGroup _messageContent;
    [SerializeField] private Button _sendButton;
    [SerializeField] private TextMeshProUGUI _errorMessage;

    private DHController _dhController;
    private Sender _sender;
    private RC4 _rc4;

    private byte[] KeyBytes => 
        _dhController.SessionKey == null || _dhController.SessionKey.Length == 0 ? 
        System.Text.Encoding.UTF8.GetBytes(_key.text) :
        _dhController.SessionKey;

    private void Awake()
    {
        _sendButton.onClick.AddListener(SendEncryptedMessage);
        _errorMessage.enabled = false;

        _sender = FindObjectOfType<Sender>();
        _dhController = FindObjectOfType<DHController>();
        _dhController.OnDhUsageChanged += UpdateUI;
        UpdateUI();
    }
    
    private void SetupCyphers()
    {
        _rc4 = new RC4(KeyBytes);
    }

    private void UpdateUI()
    {
        _sendButton.interactable = _dhController.IsDhReady;
    }

    private void SendEncryptedMessage()
    {
        string message = _messageInput.text;
        byte[] encryptedMessage = GetEncryptedMessage(message);
        
        // Enviar a mensagem cifrada para o endereço IP de destino
        _sender.SendDataInNetwork(encryptedMessage);

        // Process the received message
        _messageContent.AddMessage(message, MessageType.Sent);
        _messageInput.text = string.Empty;
    }

    public byte[] GetEncryptedMessage(string message)
    {
        SetupCyphers();

        byte[] encryptedMessage = new byte[] { };
        switch (_cypherMode.value)
        {
            case SDES_OPTION:
            case EBC_OPTION:
                encryptedMessage = SDES.Encrypt(message, KeyBytes);
                break;
            case RC4_OPTION:
                byte[] messageBytes = System.Text.Encoding.UTF8.GetBytes(message);
                encryptedMessage = _rc4.Encrypt(messageBytes);
                break;
            case CBC_OPTION:
                encryptedMessage = SDES.Encrypt(message, KeyBytes, true);
                break;
        }

        return encryptedMessage;
    }

    public string DecryptMessage(byte[] encryptedMessage)
    {
        switch (_cypherMode.value)
        {
            case SDES_OPTION:
            case EBC_OPTION:
                return SDES.Decrypt(encryptedMessage, KeyBytes);
            case RC4_OPTION:
                SetupCyphers();
                return _rc4.Decrypt(encryptedMessage);
            case CBC_OPTION:
                return SDES.Decrypt(encryptedMessage, KeyBytes, true);
        }

        return "<color=Red>ERRO:</color> Não foi escolhido modo de cifra";
    }
}
