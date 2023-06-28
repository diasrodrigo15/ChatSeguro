using UnityEngine;

namespace DefaultNamespace
{
    public enum MessageType
    {
        Received, Sent
    }
    
    public class MessageContentGroup : MonoBehaviour
    {
        [SerializeField] private MessageBox _sentMessageBox;
        [SerializeField] private MessageBox _receivedMessageBox;

        public void AddMessage(string text, MessageType messageType)
        {
            MessageBox prefab = messageType == MessageType.Received ? _receivedMessageBox : _sentMessageBox;
            MessageBox box = Instantiate(prefab, transform);
            box.SetText(text);
        }
    }
}