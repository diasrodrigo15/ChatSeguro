using System;
using UnityEngine;

public enum MessageType
{
    Received,
    Sent
}

public class MessageContentGroup : MonoBehaviour
{
    [SerializeField] private MessageBox _sentMessageBox;
    [SerializeField] private MessageBox _receivedMessageBox;

    private bool _pendingMessage = false;
    private MessageBox _prefab;
    private string _message;
    
    public void AddMessage(string text, MessageType messageType)
    {
        _prefab = messageType == MessageType.Received ? _receivedMessageBox : _sentMessageBox;
        _pendingMessage = true;
        _message = text;
    }

    private void Update()
    {
        if (_pendingMessage)
        {
            _pendingMessage = false;
            
            MessageBox box = Instantiate(_prefab, transform);
            box.SetText(_message);
        }
    }
}
