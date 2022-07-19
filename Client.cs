using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace Server
{
    class Player
    {
        public int ID;

        public Player(int _ID)
        {
            ID = _ID;
        }
    }

    class Client
    {
        public int userID;
        public bool isOnline = false;
        public bool authorized = false;

        public HashSet<string> allFriendsofuser = new HashSet<string>();

        public string playFabId;
        public string playFabNetworkId;
        public string playFabDisplayName;

        public TcpClient socket;
        public NetworkStream stream;
        public SslStream sslStream;

        public ByteBuffer buffer;
        public Player player;

        private byte[] receiveBuffer;

        public void StartClient()
        {
            socket.ReceiveBufferSize = 4096;
            socket.SendBufferSize = 4096;
            socket.ReceiveTimeout = 5000;

            stream = socket.GetStream();
            sslStream = new SslStream(stream, false);

            var certificate = new X509Certificate2(Constants.CERT_FILE, Constants.CERT_PWD);

            //if(certificate != null) { Console.WriteLine(certificate.ToString()); }


            try
            {
                sslStream.AuthenticateAsServer(certificate,
                clientCertificateRequired: false, 
                checkCertificateRevocation: true);
            }
            catch(Exception e)
            {
                Console.WriteLine(e.ToString());
                CloseConnection();
                return;
            }


            //DisplaySecurityLevel(sslStream); // TODO Remove
            //DisplaySecurityServices(sslStream);
            //DisplayCertificateInformation(sslStream);
            //DisplayStreamProperties(sslStream);

            receiveBuffer = new byte[socket.ReceiveBufferSize];
            sslStream.BeginRead(receiveBuffer, 0, socket.ReceiveBufferSize, 
                ReceivedData, null);
            player = new Player(userID);
            ServerSend.ServerCredentialRequest(userID);
            //ServerSend.Welcome(userID, "Connected to Metagamez.net service.");
        }

        private void ReceivedData(IAsyncResult _result)
        {
            try
            {
                Console.WriteLine($"Received Data from user {userID}");
                int _byteLenght = sslStream.EndRead(_result);
                if(_byteLenght <= 0) { CloseConnection(); return; }

                byte[] _tempBuffer = new byte[_byteLenght];
                Array.Copy(receiveBuffer, _tempBuffer, _byteLenght);

                ServerHandle.HandleData(userID, _tempBuffer);
                if(socket != null)
                sslStream.BeginRead(receiveBuffer, 0, socket.ReceiveBufferSize, 
                    ReceivedData, null);
            }
            catch (Exception _ex)
            {
                Console.WriteLine($"Error while receiving data: {_ex}");
                CloseConnection();
                return;
            }
        }

        public void CloseConnection()
        {
            Console.WriteLine(
                $"Connection from {socket.Client.RemoteEndPoint} has been terminated");
            sslStream.Close();
            playFabId = null;
            playFabDisplayName = null;
            playFabNetworkId = null;
            sslStream = null;
            player = null;
            isOnline = false;
            authorized = false;
            socket.Close();
            socket = null;

        }

        static void DisplaySecurityLevel(SslStream stream)
        {
            Console.WriteLine("Cipher: {0} strength {1}", stream.CipherAlgorithm, stream.CipherStrength);
            Console.WriteLine("Hash: {0} strength {1}", stream.HashAlgorithm, stream.HashStrength);
            Console.WriteLine("Key exchange: {0} strength {1}", stream.KeyExchangeAlgorithm, stream.KeyExchangeStrength);
            Console.WriteLine("Protocol: {0}", stream.SslProtocol);
        }
        static void DisplaySecurityServices(SslStream stream)
        {
            Console.WriteLine("Is authenticated: {0} as server? {1}", stream.IsAuthenticated, stream.IsServer);
            Console.WriteLine("IsSigned: {0}", stream.IsSigned);
            Console.WriteLine("Is Encrypted: {0}", stream.IsEncrypted);
        }
        static void DisplayStreamProperties(SslStream stream)
        {
            Console.WriteLine("Can read: {0}, write {1}", stream.CanRead, stream.CanWrite);
            Console.WriteLine("Can timeout: {0}", stream.CanTimeout);
        }
        static void DisplayCertificateInformation(SslStream stream)
        {
            Console.WriteLine("Certificate revocation list checked: {0}", stream.CheckCertRevocationStatus);

            X509Certificate localCertificate = stream.LocalCertificate;
            if (stream.LocalCertificate != null)
            {
                Console.WriteLine("Local cert was issued to {0} and is valid from {1} until {2}.",
                    localCertificate.Subject,
                    localCertificate.GetEffectiveDateString(),
                    localCertificate.GetExpirationDateString());
            }
            else
            {
                Console.WriteLine("Local certificate is null.");
            }
            // Display the properties of the client's certificate.
            X509Certificate remoteCertificate = stream.RemoteCertificate;
            if (stream.RemoteCertificate != null)
            {
                Console.WriteLine("Remote cert was issued to {0} and is valid from {1} until {2}.",
                    remoteCertificate.Subject,
                    remoteCertificate.GetEffectiveDateString(),
                    remoteCertificate.GetExpirationDateString());
            }
            else
            {
                Console.WriteLine("Remote certificate is null.");
            }
        }
    }
}
