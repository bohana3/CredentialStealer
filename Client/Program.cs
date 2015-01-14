using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class Client
{
    private const int PORT = 80;
    private const string IP = "127.0.0.1";
    private const string EOF = "<EOF>";
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
        return truncatedResult;
    }

    public static void write(NetworkStream clientStream, Encoding encoder, byte[] buffer)
    {
        byte[] plainBuffer = concatenateBytes(buffer, encoder.GetBytes(EOF));
        clientStream.Write(plainBuffer, 0, plainBuffer.Length);
        clientStream.Flush();
    }

    public static byte[] concatenateBytes(byte[] a, byte[] b)
    {
        byte[] c = new byte[a.Length + b.Length];
        System.Buffer.BlockCopy(a, 0, c, 0, a.Length);
        System.Buffer.BlockCopy(b, 0, c, a.Length, b.Length); 

        return c;
    }

    public Client()
    {
        IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Parse(IP), PORT);
        try
        {
            TcpClient client = new TcpClient();
            client.Connect(serverEndPoint);
            NetworkStream clientStream = client.GetStream();
            ASCIIEncoding encoder = new ASCIIEncoding();
            Client.write(clientStream, encoder, encoder.GetBytes("Echo"));
            Console.WriteLine(encoder.GetString(read(clientStream, encoder)));
            client.Close();
        }
        catch (Exception exception)
        {
            throw exception;
        }
    }
    public static void Main(string[] args)
    {
        new Client();
    }
}