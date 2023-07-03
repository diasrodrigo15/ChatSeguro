using System;
using System.Numerics;
using System.Security.Cryptography;

public class DHKeyExchange
{
    private readonly BigInteger Prime = new BigInteger(197); // Número primo
    private readonly BigInteger Generator = new BigInteger(7); // raiz primitiva

    private string privateKey;
    private string publicKey;
    private string sharedKey;

    public void GeneratePublicKey()
    {
        using (var rng = new RNGCryptoServiceProvider())
        {
            byte[] randomBytes = new byte[32];
            rng.GetBytes(randomBytes);
            privateKey = new BigInteger(randomBytes).ToString();

            BigInteger privateKeyBigInt = BigInteger.Parse(privateKey);
            publicKey = BigInteger.ModPow(Generator, privateKeyBigInt, Prime).ToString();
        }
    }

    public string GetPublicKey()
    {
        return publicKey;
    }

    public void CalculateSharedKey(string otherPartyPublicKey)
    {
        BigInteger otherPublicKeyBigInt = BigInteger.Parse(otherPartyPublicKey);
        BigInteger privateKeyBigInt = BigInteger.Parse(privateKey);

        sharedKey = BigInteger.ModPow(otherPublicKeyBigInt, privateKeyBigInt, Prime).ToString();
    }

    public byte[] DeriveSessionKey()
    {
        byte[] sharedKeyBytes = ConvertToByteArray(sharedKey);

        using (var sha256 = SHA256.Create())
        {
            byte[] sessionKey = sha256.ComputeHash(sharedKeyBytes);
            return sessionKey;
        }
    }

    private byte[] ConvertToByteArray(string value)
    {
        BigInteger bigInteger = BigInteger.Parse(value);
        byte[] bytes = bigInteger.ToByteArray();

        // Remove leading zero byte if present
        if (bytes.Length > 0 && bytes[0] == 0x00)
            Array.Copy(bytes, 1, bytes, 0, bytes.Length - 1);

        return bytes;
    }
}