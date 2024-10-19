using ChatServer.Helpers;
using System.Net;
using System.Net.Sockets;

namespace ChatServer
{
    internal class Program
    {
        static List<Client> _users = new List<Client>();
        static TcpListener _listener;
        static void Main(string[] args)
        {
            _listener = new TcpListener(IPAddress.Parse("127.0.0.1"), 7891);
            _listener.Start();

            while (true)
            {
                Client client = new Client(_listener.AcceptTcpClient());
                _users.Add(client);
                BroadcastConnection();
            }
        }

        private static void BroadcastConnection()
        {
            foreach (var user in _users)
            {
                foreach (var usr in _users)
                {
                    var broadcastPacket = new PacketBuilder();
                    broadcastPacket.WriteOpCode(1);
                    broadcastPacket.WriteMsg(usr.UserName.ToString());
                    broadcastPacket.WriteMsg(usr.UID.ToString());
                    user.ClientSocket.Client.Send(broadcastPacket.GetPacketBytes());
                }
            }
        }

        public static void BroadcastMessage(string msg)
        {
            foreach (var user in _users)
            {
                var broadcastPacket = new PacketBuilder();
                broadcastPacket.WriteOpCode(5);
                broadcastPacket.WriteMsg(msg);
                user.ClientSocket.Client.Send(broadcastPacket.GetPacketBytes());
            }
        }

        public static void SendPrivateMessage(string target, string msg)
        {
            var broadcastPacket = new PacketBuilder();
            broadcastPacket.WriteOpCode(5);
            broadcastPacket.WriteMsg(msg);
            foreach (var user in _users)
            {
                if (user.UID.ToString() == target)
                {
                    user.ClientSocket.Client.Send(broadcastPacket.GetPacketBytes());
                }
            }
        }

        public static void BroadcastDisconnect(string UID)
        {
            var disconnectedUser = _users.Where(x => x.UID.ToString() == UID).FirstOrDefault();
            _users.Remove(disconnectedUser);
            foreach (var user in _users)
            {
                var broadcastPacket = new PacketBuilder();
                broadcastPacket.WriteOpCode(10);
                broadcastPacket.WriteMsg(UID);
                user.ClientSocket.Client.Send(broadcastPacket.GetPacketBytes());
            }

            BroadcastMessage($"[{disconnectedUser.UserName}] Disconnected.");
        }
    }
}
