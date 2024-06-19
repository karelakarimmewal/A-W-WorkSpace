using System;
using System.IO;
using System.Security.Cryptography;

public class CustomEncoder
{
    private readonly byte[] key;
    private readonly byte[] iv;

    public CustomEncoder()
    {
        // Generate a new key and IV
        using (Aes aes = Aes.Create())
        {
            aes.GenerateKey();
            aes.GenerateIV();
            key = aes.Key;
            iv = aes.IV;
        }
    }

    public CustomEncoder(byte[] key, byte[] iv)
    {
        this.key = key;
        this.iv = iv;
    }

    public string Encrypt(string plainText)
    {
        if (plainText == null || plainText.Length <= 0)
            throw new ArgumentNullException(nameof(plainText));

        // Truncate the plainText to ensure the encoded result stays within 255 characters
        int maxLength = 190; // Adjust as necessary
        if (plainText.Length > maxLength)
        {
            plainText = plainText.Substring(0, maxLength);
        }

        byte[] encrypted;

        using (Aes aes = Aes.Create())
        {
            aes.Key = key;
            aes.IV = iv;
            ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

            using (MemoryStream msEncrypt = new MemoryStream())
            {
                using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                {
                    using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                    {
                        swEncrypt.Write(plainText);
                    }
                    encrypted = msEncrypt.ToArray();
                }
            }
        }

        return Convert.ToBase64String(encrypted);
    }

    public string Decrypt(string cipherText)
    {
        if (cipherText == null || cipherText.Length <= 0)
            throw new ArgumentNullException(nameof(cipherText));

        string plainText = null;
        byte[] cipherBytes = Convert.FromBase64String(cipherText);

        using (Aes aes = Aes.Create())
        {
            aes.Key = key;
            aes.IV = iv;
            ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

            using (MemoryStream msDecrypt = new MemoryStream(cipherBytes))
            {
                using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                {
                    using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                    {
                        plainText = srDecrypt.ReadToEnd();
                    }
                }
            }
        }

        return plainText;
    }

    public byte[] GetKey()
    {
        return key;
    }

    public byte[] GetIV()
    {
        return iv;
    }
}
