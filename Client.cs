using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

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

        public string playFabId;
        public string playFabNetworkId;
        public string playFabDisplayName;

        public TcpClient socket;
        public NetworkStream stream;

        public ByteBuffer buffer;
        public Player player;

        private byte[] receiveBuffer;

        public void StartClient()
        {
            socket.ReceiveBufferSize = 4096;
            socket.SendBufferSize = 4096;

            stream = socket.GetStream();
            receiveBuffer = new byte[socket.ReceiveBufferSize];
            stream.BeginRead(receiveBuffer, 0, socket.ReceiveBufferSize, 
                ReceivedData, null);
            player = new Player(userID);
            ServerSend.ServerCredentialRequest(userID);
            //ServerSend.Welcome(userID, "Connected to Metagamez.net service.");
        }

        private void ReceivedData(IAsyncResult _result)
        {
            try
            {
                Console.WriteLine("Received Data...");
                int _byteLenght = stream.EndRead(_result);
                if(_byteLenght <= 0) { CloseConnection(); return; }

                byte[] _tempBuffer = new byte[_byteLenght];
                Array.Copy(receiveBuffer, _tempBuffer, _byteLenght);

                ServerHandle.HandleData(userID, _tempBuffer);
                if(socket != null)
                stream.BeginRead(receiveBuffer, 0, socket.ReceiveBufferSize, 
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

            player = null;
            isOnline = false;
            authorized = false;
            socket.Close();
            socket = null;

        }
    }
}
