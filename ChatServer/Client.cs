using ChatServer.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ChatServer
{
    internal class Client
    {
        public string UserName { get; set; }
        public Guid UID { get; set; }
        public TcpClient ClientSocket { get; set; }

        PacketReader _packetReader;
        public Client(TcpClient client)
        {
            ClientSocket = client;
            UID = Guid.NewGuid();
            _packetReader = new PacketReader(ClientSocket.GetStream());

            byte opcode = _packetReader.ReadByte();
            UserName = _packetReader.ReadMsg();

            Console.WriteLine($"[{DateTime.Now}]: {UserName} Connected.");

            Task.Run(() => Process());

        }

        void Process()
        {
            while (true)
            {
                try
                {
                    var opcode = _packetReader.ReadByte();
                    switch (opcode)
                    {
                        case 5:
                            var msg = _packetReader.ReadMsg();
                            Console.WriteLine($"[{DateTime.Now}]: {msg} Recieved.");
                            Program.BroadcastMessage($"[{DateTime.Now}] [{UserName}]: {msg} ");
                            break;
                        default:
                            break;
                    }
                }
                catch (Exception)
                {
                    Console.WriteLine($"[{UID}: Disconnected!]");
                    Program.BroadcastDisconnect(UID.ToString());
                    ClientSocket.Close();
                    break;
                }
            }
        }
    }
}
