using System.Text;

public class RC4
{
    private byte[] byteMessage;
    private int i;
    private int j;

    public RC4(byte[] key)
    {
        byteMessage = new byte[256];
        for (int k = 0; k < 256; k++)
        {
            byteMessage[k] = (byte)k;
        }

        int keyLength = key.Length;
        j = 0;

        for (int k = 0; k < 256; k++)
        {
            j = (j + byteMessage[k] + key[k % keyLength]) % 256;
            Swap(k, j);
        }

        i = j = 0;
    }

    private void Swap(int a, int b)
    {
        byte temp = byteMessage[a];
        byteMessage[a] = byteMessage[b];
        byteMessage[b] = temp;
    }

    public byte[] Encrypt(byte[] data)
    {
        byte[] encrypted = new byte[data.Length];

        for (int k = 0; k < data.Length; k++)
        {
            i = (i + 1) % 256;
            j = (j + byteMessage[i]) % 256;
            Swap(i, j);
            int t = (byteMessage[i] + byteMessage[j]) % 256;
            encrypted[k] = (byte)(data[k] ^ byteMessage[t]);
        }

        return encrypted;
    }
    
    public string Decrypt(byte[] ciphertext)
    {
        string result = Encoding.UTF8.GetString(Encrypt(ciphertext));
        return result; 
    }
}
