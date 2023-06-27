using System;

public class SDES
{
    private static readonly int[] P10 = { 3, 5, 2, 7, 4, 10, 1, 9, 8, 6 };
    private static readonly int[] P8 = { 6, 3, 7, 4, 8, 5, 10, 9 };
    private static readonly int[] IP = { 2, 6, 3, 1, 4, 8, 5, 7 };
    private static readonly int[] IP_INVERSE = { 4, 1, 3, 5, 7, 2, 8, 6 };
    private static readonly int[] EP = { 4, 1, 2, 3, 2, 3, 4, 1 };
    private static readonly int[,] S0 = { { 1, 0, 3, 2 }, { 3, 2, 1, 0 }, { 0, 2, 1, 3 }, { 3, 1, 3, 2 } };
    private static readonly int[,] S1 = { { 0, 1, 2, 3 }, { 2, 0, 1, 3 }, { 3, 0, 1, 0 }, { 2, 1, 0, 3 } };
    private static readonly int[] P4 = { 2, 4, 3, 1 };

    public byte[] Encrypt(byte[] plaintext, byte[] key)
    {
        byte[] cipher = new byte[2];

        // Permutation P10
        byte[] p10 = new byte[10];
        for (int i = 0; i < 10; i++)
        {
            p10[i] = key[P10[i] - 1];
        }

        // Split into left and right
        byte[] left = new byte[5];
        byte[] right = new byte[5];
        Array.Copy(p10, 0, left, 0, 5);
        Array.Copy(p10, 5, right, 0, 5);

        // Generate first round keys
        byte[] round1Key = GenerateRoundKey(left, right, 1);
        byte[] round2Key = GenerateRoundKey(left, right, 2);

        // Initial permutation (IP)
        byte[] ip = new byte[8];
        for (int i = 0; i < 8; i++)
        {
            ip[i] = plaintext[IP[i] - 1];
        }

        // Split into left and right
        Array.Copy(ip, 0, left, 0, 4);
        Array.Copy(ip, 4, right, 0, 4);

        // Expansion permutation (EP)
        byte[] expanded = new byte[8];
        for (int i = 0; i < 8; i++)
        {
            expanded[i] = right[EP[i] - 1];
        }

        // XOR expanded and round1Key
        for (int i = 0; i < 8; i++)
        {
            expanded[i] ^= round1Key[i];
        }

        // S-Box substitution
        byte[] sBoxOutput = new byte[4];
        sBoxOutput[0] = (byte)((S0[expanded[0] * 2 + expanded[3], expanded[1] * 2 + expanded[2]] << 1) & 0x0F);
        sBoxOutput[1] = (byte)((S1[expanded[4] * 2 + expanded[7], expanded[5] * 2 + expanded[6]] << 1) & 0x0F);

        // Permutation P4
        byte[] p4 = new byte[4];
        for (int i = 0; i < 4; i++)
        {
            p4[i] = sBoxOutput[P4[i] - 1];
        }

        // XOR with left
        for (int i = 0; i < 4; i++)
        {
            p4[i] ^= left[i];
        }

        // Combine left and right
        byte[] combined = new byte[8];
        Array.Copy(right, 0, combined, 0, 4);
        Array.Copy(p4, 0, combined, 4, 4);

        // Second round: swap left and right
        byte[] round2 = new byte[8];
        Array.Copy(combined, 4, round2, 0, 4);
        Array.Copy(combined, 0, round2, 4, 4);

        // Initial permutation inverse (IP^-1)
        byte[] ipInverse = new byte[8];
        for (int i = 0; i < 8; i++)
        {
            ipInverse[i] = round2[IP_INVERSE[i] - 1];
        }

        // Set cipher
        cipher[0] = ipInverse[0];
        cipher[1] = ipInverse[1];

        return cipher;
    }

    private byte[] GenerateRoundKey(byte[] left, byte[] right, int round)
    {
        byte[] shiftedLeft = new byte[5];
        byte[] shiftedRight = new byte[5];

        // Shift left by 1 or 2 bits depending on the round
        for (int i = 0; i < 5; i++)
        {
            shiftedLeft[i] = left[(i + round) % 5];
            shiftedRight[i] = right[(i + round) % 5];
        }

        // Combine shifted left and shifted right
        byte[] combined = new byte[10];
        Array.Copy(shiftedLeft, 0, combined, 0, 5);
        Array.Copy(shiftedRight, 0, combined, 5, 5);

        // Permutation P8
        byte[] roundKey = new byte[8];
        for (int i = 0; i < 8; i++)
        {
            roundKey[i] = combined[P8[i] - 1];
        }

        return roundKey;
    }
}
