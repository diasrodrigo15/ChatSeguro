using TMPro;
using UnityEngine;

public class MessageBox : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _messageBox;

    public void SetText(string text)
    {
        _messageBox.text = text;
    }
}
