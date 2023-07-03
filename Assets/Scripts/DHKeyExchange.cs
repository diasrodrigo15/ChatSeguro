using System;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;

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
            privateKey =  BigInteger.Abs(new BigInteger(randomBytes)).ToString();

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
        return ConvertToByteArray(sharedKey);
    }

    private byte[] ConvertToByteArray(string value)
    {
        return Encoding.UTF8.GetBytes(value);
    }
}