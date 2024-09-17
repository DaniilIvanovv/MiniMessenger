using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace MiniMessengerServer
{
    class Program
    {
        // Словарь для хранения имен пользователей и их подключений
        static Dictionary<TcpClient, string> clientNames = new Dictionary<TcpClient, string>();

        static void Main(string[] args)
        {
            // Адрес и порт для прослушивания
            string address = "127.0.0.1"; // Локальный адрес
            int port = 8080;

            // Создание TCP-сервера
            TcpListener server = new TcpListener(IPAddress.Parse(address), port);

            // Начало прослушивания соединений
            server.Start();

            Console.WriteLine("Сервер запущен. Ожидание подключений...");

            // Цикл обработки подключений
            while (true)
            {
                // Ожидание подключения
                TcpClient client = server.AcceptTcpClient();
                Console.WriteLine("Подключение от " + client.Client.RemoteEndPoint);

                // Запуск нового потока для обработки клиента
                Task.Run(() => HandleClient(client));
            }
        }

        // Метод обработки клиента
        private static void HandleClient(TcpClient client)
        {
            try
            {
                // Получение потока для обмена данными
                NetworkStream stream = client.GetStream();

                // Получение имени пользователя
                byte[] buffer = new byte[1024];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                string userName = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                Console.WriteLine("Получено имя пользователя: " + userName);

                // Сохранение имени пользователя
                clientNames[client] = userName;

                // Цикл обработки сообщений
                while (true)
                {
                    // Получение сообщения от клиента
                    buffer = new byte[1024];
                    bytesRead = stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead == 0)
                    {
                        // Клиент отключился
                        break;
                    }

                    // Преобразование байтов в строку
                    string message = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                    Console.WriteLine("Получено сообщение от клиента: " + message);

                    // Отправка сообщения всем подключенным клиентам
                    Console.WriteLine("Отправка сообщений всем клиентам..."); // Добавьте временное сообщение
                    foreach (var connectedClient in clientNames)
                    {
                        if (connectedClient.Key == client) // Проверка, что это не сам отправитель
                        {
                            // Пропустить отправку сообщения этому клиенту 
                            continue;
                        }
                        try
                        {
                            // Измените имя на "clientStream"
                            NetworkStream clientStream = connectedClient.Key.GetStream();
                            string responseMessage = userName + ": " + message;
                            byte[] responseData = Encoding.ASCII.GetBytes(responseMessage);
                            clientStream.Write(responseData, 0, responseData.Length);

                            Console.WriteLine("Сообщение отправлено: " + responseMessage); // Добавьте временное сообщение
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Ошибка отправки сообщения: " + ex.Message);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка: " + ex.Message);
            }
            finally
            {
                // Удаление имени пользователя из словаря
                clientNames.Remove(client);

                // Закрытие соединения
                client.Close();
                Console.WriteLine("Соединение с клиентом закрыто.");
            }
        }
    }
}
