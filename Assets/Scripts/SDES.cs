using System.Text;

public class SDES
{
    private const int KeySize = 10;
    private const int BlockSize = 8;
    private const int SubKeySize = 8;
    private const int NumRounds = 2;

    private static readonly int[] P10 = { 2, 4, 1, 6, 3, 9, 0, 8, 7, 5 };
    private static readonly int[] P8 = { 5, 2, 6, 3, 7, 4, 9, 8 };
    private static readonly int[] IP = { 1, 5, 2, 0, 3, 7, 4, 6 };
    private static readonly int[] IPInverse = { 3, 0, 2, 4, 6, 1, 7, 5 };
    private static readonly int[] EP = { 3, 0, 1, 2, 1, 2, 3, 0 };
    private static readonly int[] P4 = { 1, 3, 2, 0 };

    private static readonly int[][] S0 =
    {
        new int[] { 1, 0, 3, 2 },
        new int[] { 3, 2, 1, 0 },
        new int[] { 0, 2, 1, 3 },
        new int[] { 3, 1, 3, 2 }
    };

    private static readonly int[][] S1 =
    {
        new int[] { 0, 1, 2, 3 },
        new int[] { 2, 0, 1, 3 },
        new int[] { 3, 0, 1, 0 },
        new int[] { 2, 1, 0, 3 }
    };

    private static byte[] StringToByteArray(string str)
    {
        return Encoding.ASCII.GetBytes(str);
    }

    private static string ByteArrayToString(byte[] bytes)
    {
        return Encoding.ASCII.GetString(bytes);
    }

    private static byte Permute(byte input, int[] table, int size)
    {
        byte output = 0;
        for (int i = 0; i < size; i++)
        {
            output <<= 1;
            output |= (byte)((input >> (table[i] ^ (size - 1))) & 0x01);
        }
        return output;
    }

    private static void Split(byte input, out byte left, out byte right, int size)
    {
        left = (byte)(input >> size);
        right = (byte)(input & ((1 << size) - 1));
    }

    private static byte Merge(byte left, byte right, int size)
    {
        return (byte)((left << size) | right);
    }

    private static byte CircularLeftShift(byte input, int count, int size)
    {
        return (byte)(((input << count) | (input >> (size - count))) & ((1 << size) - 1));
    }

    private static byte SBoxLookup(int[][] sBox, byte input)
    {
        int row = ((input & 0x08) >> 2) | (input & 0x01);
        int col = (input & 0x06) >> 1;
        return (byte)sBox[row][col];
    }

    private static byte FunctionFk(byte input, byte subKey)
    {
        byte expanded = Permute(input, EP, SubKeySize);
        byte xored = (byte)(expanded ^ subKey);
        byte left, right;
        Split(xored, out left, out right, SubKeySize / 2);

        byte substitutedLeft = SBoxLookup(S0, left);
        byte substitutedRight = SBoxLookup(S1, right);

        byte substituted = Merge(substitutedLeft, substitutedRight, SubKeySize / 2);
        return Permute(substituted, P4, SubKeySize / 2);
    }

    private static byte[] GenerateSubKeys(byte key)
    {
        byte permutedKey = Permute(key, P10, KeySize);
        byte left, right;
        Split(permutedKey, out left, out right, KeySize / 2);

        byte shiftedLeft1 = CircularLeftShift(left, 1, KeySize / 2);
        byte shiftedRight1 = CircularLeftShift(right, 1, KeySize / 2);
        byte subKey1 = Permute(Merge(shiftedLeft1, shiftedRight1, KeySize / 2), P8, SubKeySize);

        byte shiftedLeft2 = CircularLeftShift(shiftedLeft1, 2, KeySize / 2);
        byte shiftedRight2 = CircularLeftShift(shiftedRight1, 2, KeySize / 2);
        byte subKey2 = Permute(Merge(shiftedLeft2, shiftedRight2, KeySize / 2), P8, SubKeySize);

        return new byte[] { subKey1, subKey2 };
    }

    public static byte[] Encrypt(string plaintext, string key, bool useCBC = false, byte[] iv = null)
    {
        byte[] plaintextBytes = Encoding.ASCII.GetBytes(plaintext);
        byte[] keyBytes = Encoding.ASCII.GetBytes(key);

        byte[] encryptedBytes = new byte[plaintextBytes.Length];

        byte previousBlock = 0x00;

        for (int i = 0; i < plaintextBytes.Length; i++)
        {
            byte currentBlock = plaintextBytes[i];

            if (useCBC)
            {
                if (iv != null && i == 0)
                {
                    currentBlock ^= iv[0];
                }
                else
                {
                    currentBlock ^= previousBlock;
                }
            }

            byte[] subKeys = GenerateSubKeys(keyBytes[0]);

            byte permutedPlaintext = Permute(currentBlock, IP, BlockSize);
            byte left, right;
            Split(permutedPlaintext, out left, out right, BlockSize / 2);

            for (int round = 0; round < NumRounds; round++)
            {
                byte fResult = FunctionFk(right, subKeys[round]);
                byte xored = (byte)(left ^ fResult);
                left = right;
                right = xored;
            }

            byte swapped = Merge(right, left, BlockSize / 2);
            byte encryptedBlock = Permute(swapped, IPInverse, BlockSize);

            encryptedBytes[i] = encryptedBlock;

            previousBlock = encryptedBlock;
        }

        return encryptedBytes;
    }


    public static string Decrypt(byte[] ciphertext, string key, bool useCBC = false, byte[] iv = null)
    {
        byte[] keyBytes = Encoding.ASCII.GetBytes(key);

        byte[] decryptedBytes = new byte[ciphertext.Length];

        byte previousBlock = 0x00;

        for (int i = 0; i < ciphertext.Length; i++)
        {
            byte currentBlock = ciphertext[i];

            byte[] subKeys = GenerateSubKeys(keyBytes[0]);

            byte permutedCiphertext = Permute(currentBlock, IP, BlockSize);
            byte left, right;
            Split(permutedCiphertext, out left, out right, BlockSize / 2);

            for (int round = NumRounds - 1; round >= 0; round--)
            {
                byte fResult = FunctionFk(right, subKeys[round]);
                byte xored = (byte)(left ^ fResult);
                left = right;
                right = xored;
            }

            byte swapped = Merge(right, left, BlockSize / 2);
            byte decryptedBlock = Permute(swapped, IPInverse, BlockSize);

            if (useCBC)
            {
                if (iv != null && i == 0)
                {
                    decryptedBlock ^= iv[0];
                }
                else
                {
                    decryptedBlock ^= previousBlock;
                }
            }

            decryptedBytes[i] = decryptedBlock;

            previousBlock = currentBlock;
        }

        return ByteArrayToString(decryptedBytes);
    }
}