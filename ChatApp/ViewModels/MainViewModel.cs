using ChatClient.Commands;
using ChatClient.Helpers;
using ChatClient.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ChatClient.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        public ObservableCollection<UserModel> Users { get; set; }
        public ObservableCollection<string> Messages { get; set; }
        public RelayCommand ConnectToServerCommand { get; set; }
        public RelayCommand SendMessageCommand { get; set; }
        private Server _server;
        public string UserName { get; set; }

        private string _message;
        public string Message
        {
            get => _message;
            set
            {
                _message = value;
                OnPropertyChanged(nameof(Message));
            }
        }

        private UserModel _channel;
        public UserModel Channel
        {
            get => _channel;
            set
            {
                _channel = value;
                OnPropertyChanged(nameof(Channel));
            }
        }
        public MainViewModel()
        {
            Users = new ObservableCollection<UserModel>();
            Messages = new ObservableCollection<string>();
            _server = new Server();

            Users.Add(new UserModel
            {
                UserName = "Public",
                UID = "PUBLIC"
            });

            Channel = Users[0];

            _server.connectedEvent += UserConnected;
            _server.msgRecievedEvent += MessageRecieved;
            _server.userDisconnectEvent += RemoveUser;

            ConnectToServerCommand = new RelayCommand(o => _server.ConnectToServer(UserName), o => !string.IsNullOrEmpty(UserName));
            SendMessageCommand = new RelayCommand(o => 
            { 
                _server.SendMsgToServer(Channel.UID, Message);
                Message = String.Empty;
            }, 
            o => !string.IsNullOrEmpty(Message));
        }

        private void RemoveUser()
        {
            var UID = _server._packetReader.ReadMsg();
            var user = Users.Where(x => x.UID == UID).FirstOrDefault();
            Application.Current.Dispatcher.Invoke(() => Users.Remove(user));
        }

        private void MessageRecieved()
        {
            var msg = _server._packetReader.ReadMsg();
            Application.Current.Dispatcher.Invoke(() => Messages.Add(msg));
        }

        private void UserConnected()
        {
            var user = new UserModel
            {
                UserName = _server._packetReader.ReadMsg(),
                UID = _server._packetReader.ReadMsg(),
            };

            if (!Users.Any(x => x.UID == user.UID))
            {
                Application.Current.Dispatcher.Invoke(() => Users.Add(user));
            }
        }
    }
}
