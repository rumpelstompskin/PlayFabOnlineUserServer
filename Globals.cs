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

        public static ByteBuffer GetUserOnlineStatusBufferByID(string _friendPlayFabID) // TODO: Consider moving this logic out of here.
        {
            //Console.WriteLine("Getting Users Online Status..."); // Debug
            bool _status = false;
            string _playFabNetworkID = "";
            ByteBuffer _buffer = new ByteBuffer();

            for (int i = 1; i < clients.Count; i++) // Try to figure out a more efficient way of doing this.
            {
                if (clients[i].playFabId == _friendPlayFabID)
                {
                    _status = true;
                    _playFabNetworkID = clients[i].playFabNetworkId;
                    break;
                }
            }
            _buffer.WriteInt((int)ServerPackets.UserInfoRequest); // What type of packet we are transmitting to the user.
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
