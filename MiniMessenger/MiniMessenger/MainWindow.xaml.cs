using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using System.Threading.Tasks;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Windows.Forms;

namespace MiniMessenger
{
    public partial class MainWindow : Window
    {
        // Адрес и порт для отправки/приема сообщений
        private string address = "127.0.0.1"; // Локальный адрес
        private int port = 8080;

        // TCP-клиент
        private TcpClient client;

        private string userName; // Храним имя пользователя

        public MainWindow()
        {
            InitializeComponent();
            client = new TcpClient();

            // Запрашиваем имя пользователя
            userName = GetUserName();
            if (!string.IsNullOrEmpty(userName))
            {
                Connect("127.0.0.1", 8080); // Подключитесь к серверу
            }
            else
            {
                // Пользователь не ввел имя, выходим
                this.Close();
            }
        }

        private string GetUserName()
        {
            string userName = "";
            CommonOpenFileDialog inputDialog = new CommonOpenFileDialog();
            inputDialog.IsFolderPicker = false;
            inputDialog.Title = "Введите ваше имя";
            if (inputDialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                userName = inputDialog.FileName;
            }
            return userName;
        }

        private async void Connect(string address, int port)
        {
            try
            {
                await client.ConnectAsync(address, port);
                if (client.Connected)
                {
                    // Подключение успешно установлено
                    System.Windows.MessageBox.Show("Подключение установлено");

                    // Отправка имени пользователя на сервер
                    byte[] data = Encoding.ASCII.GetBytes(userName);
                    NetworkStream stream = client.GetStream();
                    await stream.WriteAsync(data, 0, data.Length);
                }
                else
                {
                    // Подключение не установлено
                    System.Windows.MessageBox.Show("Ошибка подключения");
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Ошибка подключения: " + ex.Message);
            }
        }
        private async void SendButton_Click(object sender, RoutedEventArgs e)
        {
            if (client.Connected)
            {
                // Получение сообщения из текстового поля
                string message = MessageInput.Text;

                // Преобразование сообщения в байты
                byte[] data = Encoding.ASCII.GetBytes(message);

                // Отправка сообщения
                NetworkStream stream = client.GetStream();
                await stream.WriteAsync(data, 0, data.Length);

                // Очистка текстового поля
                MessageInput.Text = "";

                // Вывод отправленного сообщения в ListBox
                MessageList.Items.Add(userName + ": " + message);
            }
            else
            {
                System.Windows.MessageBox.Show("Ошибка: Нет подключения");
            }
        }

        private async void ReceiveMessage()
        {
            if (client.Connected)
            {
                try
                {
                    // Получение сообщения
                    NetworkStream stream = client.GetStream();
                    byte[] buffer = new byte[1024];
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);

                    // Преобразование байтов в строку
                    string receivedMessage = Encoding.ASCII.GetString(buffer, 0, bytesRead);

                    // Вывод полученного сообщения в ListBox
                    MessageList.Items.Add(receivedMessage);
                }
                catch (Exception ex)
                {
                    // Обработка исключений
                    System.Windows.MessageBox.Show("Ошибка при получении сообщения: " + ex.Message);
                }
            }
            else
            {
                System.Windows.MessageBox.Show("Ошибка: Нет подключения");
            }
        }
    }
}
