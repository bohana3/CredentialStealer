using System;
using System.IO;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

public class Communication
{
    private const string PASS = "2f8RHNB44gPtaelSVztU2ZXUbuEn1OiY";
    private const string EOF = "<EOF>";
    public static byte[] Encrypt(string password, byte[] data)
    {
        RijndaelManaged rijndael = new RijndaelManaged();
        rijndael.Mode = CipherMode.CBC;
        Rfc2898DeriveBytes rfcDb = new Rfc2898DeriveBytes(password, System.Text.Encoding.UTF8.GetBytes(password));
        byte[] key = rfcDb.GetBytes(16);
        byte[] iv = rfcDb.GetBytes(16);
        ICryptoTransform aesEncryptor = rijndael.CreateEncryptor(key, iv);
        MemoryStream ms = new MemoryStream();
        CryptoStream cs = new CryptoStream(ms, aesEncryptor, CryptoStreamMode.Write);
        cs.Write(data, 0, data.Length);
        cs.FlushFinalBlock();
        byte[] CipherBytes = ms.ToArray();
        ms.Close();
        cs.Close();
        return CipherBytes;
    }

    public static byte[] Decrypt(string password, byte[] data)
    {
        RijndaelManaged rijndael = new RijndaelManaged();
        rijndael.Mode = CipherMode.CBC;
        Rfc2898DeriveBytes rfcDb = new Rfc2898DeriveBytes(password, System.Text.Encoding.UTF8.GetBytes(password));
        byte[] key = rfcDb.GetBytes(16);
        byte[] iv = rfcDb.GetBytes(16);
        ICryptoTransform decryptor = rijndael.CreateDecryptor(key, iv);
        MemoryStream ms = new MemoryStream(data);
        CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
        byte[] plainTextData = new byte[data.Length];
        int decryptedByteCount = cs.Read(plainTextData, 0, plainTextData.Length);
        ms.Close();
        cs.Close();
        byte[] result = new byte[decryptedByteCount];
        Array.Copy(plainTextData, 0, result, 0, result.Length);
        return result;
    }

    public static byte[] read(NetworkStream clientStream, Encoding encoder)
    {
        byte[] result = new byte[0];
        int tokenSize = Convert.ToBase64String(encoder.GetBytes(EOF)).Length;
        while (true)
        {
            byte[] message = new byte[4096];
            int bytesRead = 0;
            try
            {
                bytesRead = clientStream.Read(message, 0, 4096);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                break;
            }
            if (bytesRead == 0)
            {
                return result;
            }
            Array.Resize<byte>(ref result, result.Length + bytesRead);
            Array.Copy(message, 0, result, result.Length - bytesRead, bytesRead);
            if (encoder.GetString(result).EndsWith(EOF))
            {
                break;
            }
        }
        int eofLength = encoder.GetBytes(EOF).Length;
        byte[] truncatedResult = new byte[result.Length - eofLength];
        Array.Copy(result, 0, truncatedResult, 0, truncatedResult.Length);
        return Decrypt(PASS, truncatedResult);
    }

    public static byte[] concatenateBytes(byte[] one, byte[] two)
    {
        byte[] result = new byte[one.Length + two.Length];
        Array.Copy(one, 0, result, 0, one.Length);
        Array.Copy(two, 0, result, one.Length, two.Length);
        return result;
    }

    public static void write(NetworkStream clientStream, Encoding encoder, byte[] buffer)
    {
        byte[] encryptedBuffer = concatenateBytes(Encrypt(PASS, buffer), encoder.GetBytes(EOF));
        clientStream.Write(encryptedBuffer, 0, encryptedBuffer.Length);
        clientStream.Flush();
    }

    public static string readString(NetworkStream clientStream, Encoding encoder)
    {
        return encoder.GetString(read(clientStream, encoder));
    }

    public static void writeString(NetworkStream clientStream, Encoding encoder, string data)
    {
        write(clientStream, encoder, encoder.GetBytes(data));
    }
}