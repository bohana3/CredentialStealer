using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

class Serveur
{
    private const int PORT = 80;
    private const string EOF = "<EOF>";
    private TcpListener tcpListener;
    private Thread listenThread;
    private void ListenForClients()
    {
        this.tcpListener.Start();
        while (true)
        {
            TcpClient client = this.tcpListener.AcceptTcpClient();
            Thread clientThread = new Thread(new ParameterizedThreadStart(HandleClientComm));
            clientThread.Start(client);
        }
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

    private void HandleClientComm(object client)
    {
        TcpClient tcpClient = (TcpClient)client;
        NetworkStream clientStream = tcpClient.GetStream();
        ASCIIEncoding encoder = new ASCIIEncoding();
        write(clientStream, encoder, read(clientStream, encoder));
        tcpClient.Close();
    }
    public Serveur()
    {
        this.tcpListener = new TcpListener(IPAddress.Any, PORT);
        this.listenThread = new Thread(new ThreadStart(ListenForClients));
        this.listenThread.Start();
    }
    public static void Main(string[] args)
    {
        new Serveur();
    }
}