using ChatClient.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ChatClient.Helpers
{
    public class Server
    {
        TcpClient _client = new TcpClient();
        public PacketReader _packetReader;

        public event Action connectedEvent;
        public event Action msgRecievedEvent;
        public event Action userDisconnectEvent;
        public void ConnectToServer(string username)
        {
            if (!_client.Connected)
            {
                _client.Connect("127.0.0.1", 7891);
                _packetReader = new PacketReader(_client.GetStream());

                if (!string.IsNullOrEmpty(username))
                {
                    PacketBuilder connectPacket = new PacketBuilder();
                    connectPacket.WriteOpCode(0);
                    connectPacket.WriteMsg(username);
                    _client.Client.Send(connectPacket.GetPacketBytes());
                }
                ReadPackets();
            }
        }

        private void ReadPackets()
        {
            Task.Run(() => {
                while (true)
                {
                    var opcode = _packetReader.ReadByte();
                    switch (opcode)
                    {
                        case 1:
                            connectedEvent?.Invoke();
                            break;
                        case 5:
                            msgRecievedEvent?.Invoke();
                            break;
                        case 10:
                            userDisconnectEvent?.Invoke();
                            break;
                        default:
                            break;
                    }
                }
            });
        }

        public void SendMsgToServer(string channel, string msg) 
        {
            var messagePacket = new PacketBuilder();
            if (channel == "PUBLIC")
            {
                messagePacket.WriteOpCode(50);
            }
            else
            {
                messagePacket.WriteOpCode(5);
                messagePacket.WriteMsg(channel);
            }
            messagePacket.WriteMsg(msg);
            _client.Client.Send(messagePacket.GetPacketBytes());
        }
    }
}
