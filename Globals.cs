using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    internal class Globals
    {
        public static bool serverIsRunning = false;
        public static Dictionary<int, Client> clients = new Dictionary<int, Client>();

        /*
        public static bool GetUserOnlineStatus(string _playFabID)
        {
            bool status = false;

            for (int i = 0; i < clients.Count; i++)
            {
                if(clients[i].playFabId == _playFabID)
                {
                    status = true;
                }
            }

            return status;
        }
        */
        public static ByteBuffer GetUserOnlineStatusBufferByID(string _friendPlayFabID)
        {
            Console.WriteLine("Getting Users Online Status...");
            bool _status = false;
            string _playFabNetworkID = "";
            ByteBuffer _buffer = new ByteBuffer();
            for (int i = 1; i < clients.Count; i++)
            {
                if (clients[i].playFabId == _friendPlayFabID)
                {
                    _status = true;
                    _playFabNetworkID = clients[i].playFabNetworkId;
                    break;
                }
            }
            _buffer.WriteInt((int)ServerPackets.UserInfoRequest);
            //_buffer.WriteBool(true); // This tells the client it's a response. TODO: Change into an int to be able to filter more responses.
            _buffer.WriteBool(_status); // Tells our client the status of the user.
            _buffer.WriteString(_friendPlayFabID);
            if (_status == true) // If the user is online, pass along the user's network ID. TODO: Change into a request for communication to the user.
            {
                _buffer.WriteString(_playFabNetworkID);
            }
            

            return _buffer;
        }
    }
}
