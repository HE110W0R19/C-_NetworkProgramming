using ChatClient;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using Utilities;

namespace MessageSender_WPFChat
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Socket socket;
        private string chatContent;
        private Client_Broadcast user_broadcast;
        public MainWindow()
        {
            InitializeComponent();
            user_broadcast = new Client_Broadcast();
        }

        private void SendButton_Click(object sender, MouseButtonEventArgs e)
        {
            messageWork(this.socket, this.chatContent, ChatContent_RichTextBox, MessageInput_TextBox);
        }

        private void ClipButton_Click(object sender, RoutedEventArgs e)
        {
            //need make image sender
        }

        private static void messageWork(Socket socket, string chat_content, RichTextBox ChatTB, TextBox InputMessage)
        {
            chat_content = ReceiveChatContent(socket);
            ChatTB.Document = ShowChatContent(chat_content);

            string message = "";

            message = GetClientMessage(InputMessage);
            SendMessageToServer(socket, message);

            chat_content = ReceiveChatContent(socket);
            ChatTB.Document = ShowChatContent(chat_content);
        }

        private static void searchServers(Client_Broadcast client, int milliseconds)
        {
            client.Start();
            Thread.Sleep(milliseconds);
            client.Stop();
        }

        private static void DisposeClientSocket(Socket socket)
        {
            socket.Close();
            socket.Dispose();
        }

        private static void DisconnectClientFromServer(Socket socket)
        {
            socket.Disconnect(false);
            Console.WriteLine("Client disconnected from server");
        }

        private static void SendNameToServer(Socket socket, string name)
        {
            //Console.WriteLine("Sending Name to server");
            SocketUtility.SendString(socket, name,
                () => { /*Console.WriteLine($"Send string to server data check client side exception");*/ });
            //Console.WriteLine("Name sent to server");
        }

        private static void SendMessageToServer(Socket socket, string message)
        {
            //Console.WriteLine("Sending Message to server");
            SocketUtility.SendString(socket, message,
                () => { /*Console.WriteLine($"Send string to server data check client side exception");*/ });
            // Console.WriteLine("Message sent to server");
        }

        private static string GetClientMessage(TextBox InputMessage)
        {
            return InputMessage.Text;
        }

        private static FlowDocument ShowChatContent(string chatContent)
        {
            FlowDocument Chat_document = new FlowDocument();
            Paragraph Chat_paragraph = new Paragraph();
            Chat_paragraph.Inlines.Add(new Bold(new Run(chatContent)));
            Chat_document.Blocks.Add(Chat_paragraph);
            return Chat_document;
        }

        private static string ReceiveChatContent(Socket socket)
        {
            string chatContent = SocketUtility.ReceiveString(socket,
                () => { /*Console.WriteLine($"Receive string size check from server client side exception");*/ },
                () => { /*Console.WriteLine($"Receive string data check from server client side exception");*/ });
            return chatContent;
        }

        private static Socket ConnectClientToServer(IPEndPoint serverEndPoint)
        {
            Socket socket = new Socket(SocketType.Stream, ProtocolType.IP);

            socket.Connect(serverEndPoint);

            //Console.WriteLine($"Client connected Local {socket.LocalEndPoint} Remote {socket.RemoteEndPoint}");

            return socket;
        }


        private void SearchServer_Button_Click(object sender, RoutedEventArgs e)
        {
            searchServers(user_broadcast, 10000);

            var allServers = user_broadcast.getServerList();

            foreach (var server in allServers)
            {
                ServerList_ListBox.Items.Add(server);
            }
        }

        private void ConnectToServer_Button_Click(object sender, RoutedEventArgs e)
        {
            if (StartUserName_TextBox.Text.Length > 2 &&
                ServerList_ListBox.Items.Count > 0 &&
                ServerList_ListBox.SelectedIndex > -1)
            {
                try
                {
                    socket = ConnectClientToServer(new IPEndPoint(IPAddress.Loopback, Convert.ToInt32(ServerList_ListBox.SelectedValue)));
                    chatContent = ReceiveChatContent(socket);
                    ChatContent_RichTextBox.Document = ShowChatContent(chatContent);
                    SendNameToServer(socket, StartUserName_TextBox.Text);
                }
                catch (Exception excep)
                {
                    this.ChatContent_RichTextBox.AppendText(excep.Message);
                }
                this.StartUserName_Label.Visibility = Visibility.Hidden;
                this.StartUserName_TextBox.Visibility = Visibility.Hidden;
                this.StartUserImage.Visibility = Visibility.Hidden;
                this.StartHeader_label.Visibility = Visibility.Hidden;
                this.StartWindow_label.Visibility = Visibility.Hidden;
                this.ServerList_Label.Visibility = Visibility.Hidden;
                this.ServerList_ListBox.Visibility = Visibility.Hidden;
                this.SearchServer_Button.Visibility = Visibility.Hidden;
                this.ConnectToServer_Button.Visibility = Visibility.Hidden;
            }
        }
    }
}
