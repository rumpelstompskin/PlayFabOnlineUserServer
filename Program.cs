using System;
using System.Threading;
using System.Net;
using System.Net.Sockets;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            Globals.serverIsRunning = true;

            Thread _serviceThread = new Thread(new ThreadStart(ServiceLogicThread));
            _serviceThread.Start();
            General.StartServer();
        }

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
            Console.WriteLine($"Starting server on port: {port}");
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
            InitServerData();
            ServerTCP.InitNetwork();
            Console.WriteLine("Server started");
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

                    Globals.clients[_userID].stream.BeginWrite(_buffer.ToArray(), 0,
                        _buffer.ToArray().Length, null, null);
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

            _buffer.WriteString(_msg);
            _buffer.WriteInt(_sendToUser);

            SendDataTo(_sendToUser, _buffer.ToArray());
            _buffer.Dispose();
        }

        public static void OnlinePlayerCheck(int _sendToUser, bool _answer)
        {
            ByteBuffer _buffer = new ByteBuffer();
            _buffer.WriteInt((int)ServerPackets.HandShake);

            _buffer.WriteBool(_answer);
            
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
                {(int)ClientPackets.HandShakeReceived, HandShakeReceived }
            };
        }

        public static void HandShakeReceived(int _userID, byte[] _data)
        {
            ByteBuffer _buffer = new ByteBuffer();
            _buffer.WriteBytes(_data);
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

            if(packets.TryGetValue(_userID, out Packet? _packet))
            {
                _packet.Invoke(_userID, _data);
            }
        }
    }
}