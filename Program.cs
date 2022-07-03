using System;
using System.Threading;
using System.Net;
using System.Net.Sockets;

namespace Server
{
    /// <summary>
    /// Metagamez.net Online Status Service
    /// </summary>
    class Program
    {
        static void Main(string[] args) // Program's initial method call.
        {
            Globals.serverIsRunning = true;

            Thread _serviceThread = new Thread(new ThreadStart(ServiceLogicThread));
            _serviceThread.Start();

            General.StartServer();
        }

        /// <summary>
        /// Keeps the service alive.
        /// </summary>
        private static void ServiceLogicThread()
        {
            Console.WriteLine
                ($"Service Thread started. " +
                $"Running at {Constants.TICKS_PER_SEC} ticks per second");

            DateTime _lastLoop = DateTime.Now;
            DateTime _nextLoop = _lastLoop.AddMilliseconds(Constants.MS_PER_TICK);

            while (Globals.serverIsRunning)
            {
                while(_nextLoop < DateTime.Now)
                {
                    // Server tick
                    
                    _lastLoop = _nextLoop;
                    _nextLoop = _nextLoop.AddMilliseconds(Constants.MS_PER_TICK);

                    if(_nextLoop > DateTime.Now) // Sleep thread until next loop.
                    {
                        Thread.Sleep(_nextLoop - DateTime.Now);
                    }
                }
            }
        }
    }

    class ServerTCP
    {
        private static TcpListener socket = default!;
        private static int port = 16320;

        public static void InitNetwork()
        {
            Console.WriteLine($"Starting server on port: {port}"); // TODO: Evaluate if this needs to be written in a log. Probably not.
            ServerHandle.InitPackets();
            socket = new TcpListener(IPAddress.Any, port);
            socket.Start();
            socket.BeginAcceptTcpClient(new AsyncCallback(ClientConnected), null);
        }

        public static void ClientConnected(IAsyncResult _result)
        {
            TcpClient _client = socket.EndAcceptTcpClient(_result);
            _client.NoDelay = false;

            Console.WriteLine($"Incoming connection from: {_client.Client.RemoteEndPoint}");

            socket.BeginAcceptTcpClient(new AsyncCallback(ClientConnected), null);

            for (int i = 1; i <= Constants.MAX_USERS; i++)
            {
                if(Globals.clients[i].socket == null)
                {
                    Globals.clients[i].socket = _client;
                    Globals.clients[i].userID = i;
                    Globals.clients[i].StartClient();
                    return;
                }
            }

            Console.WriteLine("Server is full");
        }
    }

    class General
    {
        public static void StartServer()
        {
            InitServerData(); // Populates our dictionary
            ServerTCP.InitNetwork(); // Initialize the network
            Console.WriteLine("Server started"); // TODO: Change to a log entry
        }

        private static void InitServerData()
        {
            for (int i = 1; i < Constants.MAX_USERS; i++)
            {
                Globals.clients.Add(i, new Client());
            }
        }
    }

    class ServerSend
    {
        public static void SendDataTo(int _userID, byte[] _data)
        {
            try
            {
                if(Globals.clients[_userID].socket != null)
                {
                    ByteBuffer _buffer = new ByteBuffer();
                    _buffer.WriteInt(_data.GetUpperBound(0) - _data.GetLowerBound(0) + 1);
                    _buffer.WriteBytes(_data);

                    Globals.clients[_userID].sslStream.BeginWrite(_buffer.ToArray(), 0,
                        _buffer.ToArray().Length, f => { Console.WriteLine($"Sending Data to user {_userID}"); }, null);
                    _buffer.Dispose();
                }
            }
            catch (Exception _ex)
            {
                Console.WriteLine($"Error sending data to user {_userID}. {_ex}");
            }
        }

        public static void Welcome(int _sendToUser, string _msg)
        {
            ByteBuffer _buffer = new ByteBuffer();
            _buffer.WriteInt((int)ServerPackets.HandShake);

            //_buffer.WriteBool(false);
            _buffer.WriteString(_msg);
            _buffer.WriteInt(_sendToUser);

            SendDataTo(_sendToUser, _buffer.ToArray());
            _buffer.Dispose();
        }

        public static void ServerReturnUserStatus(int _sendToUser, string _friendPlayFabID)
        {
            ByteBuffer _buffer = Globals.GetUserOnlineStatusBufferByID(_friendPlayFabID);

            SendDataTo(_sendToUser, _buffer.ToArray());

            _buffer.Dispose();
        }

        public static void ServerCredentialRequest(int _sendToUser)
        {
            ByteBuffer _buffer = new ByteBuffer();
            _buffer.WriteInt((int)ServerPackets.AuthorizeClient);
            SendDataTo(_sendToUser, _buffer.ToArray());
            _buffer.Dispose();
        }
    }

    class ServerHandle
    {
        public delegate void Packet(int _userId, byte[] _data);
        public static Dictionary<int, Packet> packets = default!;

        public static void InitPackets()
        {
            Console.WriteLine("Initializing packets...");
            packets = new Dictionary<int, Packet>()
            {
                {(int)ClientPackets.HandShakeReceived, HandShakeReceived },
                {(int)ClientPackets.UserInfoRequestReceived,  UserInfoRequestReceived },
                {(int)ClientPackets.AuthorizeClientReceived, AuthorizationRequestReceived }
            };
        }
        /// <summary>
        /// HandShakeReceived gets called when the client send's back data.
        /// Here we can initiate our online request logic according to data from the client.
        /// </summary>
        /// <param name="_userID"></param>
        /// <param name="_data"></param>
        public static void HandShakeReceived(int _userID, byte[] _data)
        {
            ByteBuffer _buffer = new ByteBuffer();
            _buffer.WriteBytes(_data); // Taking incoming data and converting it into readable format.
            _buffer.ReadInt();

            string _username = _buffer.ReadString();
            string _playFabID = _buffer.ReadString();
            string _playFabNetworkID = _buffer.ReadString();

            _buffer.Dispose();

            Console.WriteLine(
                $"Connection from {Globals.clients[_userID].socket.Client.RemoteEndPoint}" +
                $" was successful. Username: {_username}. PlayFab ID: {_playFabID}. Network ID: {_playFabNetworkID}");

            Globals.clients[_userID].playFabDisplayName = _username;
            Globals.clients[_userID].playFabId = _playFabID;
            Globals.clients[_userID].playFabNetworkId = _playFabNetworkID;
        }

        public static void UserInfoRequestReceived(int _userID, byte[] _data)
        {
            ByteBuffer _buffer = new ByteBuffer();
            _buffer.WriteBytes(_data); // Taking incoming data and converting it into readable format.
            _buffer.ReadInt();

            //Console.WriteLine("Client information request received.");
            string _friendPlayFabID = _buffer.ReadString();

            Console.WriteLine($"Requesting information about user: {_friendPlayFabID}");
            // Call method to gather information with friend PlayFabID
            ServerSend.ServerReturnUserStatus(_userID, _friendPlayFabID);
            _buffer.Dispose();
        }

        public static void AuthorizationRequestReceived(int _userID, byte[] _data)
        {
            if(Globals.clients[_userID].authorized == true) 
            { 
                return; 
            }
            string phrase;
            string answer = "Authorization Test Key"; // TODO Change me
            ByteBuffer _buffer = new ByteBuffer();
            _buffer.WriteBytes(_data);
            _buffer.ReadInt();

            phrase = _buffer.ReadString();
            if(phrase == answer)
            {
                Globals.clients[_userID].authorized = true;
                ServerSend.Welcome(_userID, "Connected to Metagamez.net service.");
            } else
            {
                Globals.clients[_userID].CloseConnection();
            }
            _buffer.Dispose();
        }

        public static void HandleData(int _userID, byte[] _data)
        {
            byte[] _tempBuffer = (byte[])_data.Clone();
            int _packetLenght = 0;

            if(Globals.clients[_userID].buffer == null)
            {
                Globals.clients[_userID].buffer = new ByteBuffer();
            }

            Globals.clients[_userID].buffer.WriteBytes(_tempBuffer);

            if(Globals.clients[_userID].buffer.Count() == 0)
            {
                Globals.clients[_userID].buffer.Clear();
                return;
            }

            if(Globals.clients[_userID].buffer.Length() >= 4)
            {
                _packetLenght = Globals.clients[_userID].buffer.ReadInt(false);
                if(_packetLenght <= 0)
                {
                    Globals.clients[_userID].buffer.Clear();
                    return;
                }
            }

            while (_packetLenght > 0 &&
                _packetLenght <= Globals.clients[_userID].buffer.Length() - 4)
            {
                Globals.clients[_userID].buffer.ReadInt();
                _data = Globals.clients[_userID].buffer.ReadBytes(_packetLenght);
                HandlePackets(_userID, _data);

                _packetLenght = 0;

                if(Globals.clients[_userID].buffer.Length() >= 4)
                {
                    _packetLenght = Globals.clients[_userID].buffer.ReadInt(false);

                    if(_packetLenght <= 0)
                    {
                        Globals.clients[_userID].buffer.Clear();
                        return;
                    }
                }

                if(_packetLenght <= 1)
                {
                    Globals.clients[_userID].buffer.Clear();
                }
            }
        }

        public static void HandlePackets(int _userID, byte[] _data)
        {
            ByteBuffer _buffer = new ByteBuffer();
            _buffer.WriteBytes(_data);

            int _packetID = _buffer.ReadInt();
            _buffer.Dispose();

            if(packets.TryGetValue(_packetID, out Packet? _packet))
            {
                _packet.Invoke(_userID, _data);
            } else
            {
                Console.WriteLine("Invalid Packet");
            }
        }
    }
}