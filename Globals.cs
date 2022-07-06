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

        public static ByteBuffer GetMultiUserOnlineStatusBufferByID(HashSet<string> allUsersFriends)
        {
            HashSet<string> friendsCurrentlyOnline = new HashSet<string>(); // Create a hashset to store possitive results

            ByteBuffer _buffer = new ByteBuffer(); // Instantiate new ByteBuffer for transmition

            for (int i = 0; i < clients.Count; i++) // Cycle through all our online clients TODO Find better way to do this
            {
                string friendPlayFabID = clients[i].playFabId; // storing our result
                if (allUsersFriends.Contains(friendPlayFabID)) // Checking if the online user is on our friends list. TODO Find better way to do this
                {
                    friendsCurrentlyOnline.Add(friendPlayFabID); // If the online user is on our friends list, add it to our temporary hashset.
                }
            }

            _buffer.WriteInt((int)ServerPackets.UserInfoRequest); // What type of packet are we sending?
            _buffer.WriteInt(friendsCurrentlyOnline.Count); // How many of our friends are online
            foreach(string online in friendsCurrentlyOnline) // Cycle through our online friends to populate our string
            {
                _buffer.WriteString(online); // write the string to the buffer
            }

            return _buffer; // Return the buffer.
        }
    }
}
