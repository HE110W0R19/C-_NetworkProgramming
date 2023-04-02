using System.Collections.Generic;
using System.Linq;

namespace ChatServer
{
    //vvv Класс "БД" нашего чата 
    internal static class ChatDatabase
    {
        //vvv Лист(Вектор) где хранятся наши сообщения из чата
        private static List<string> _chatLines = new List<string>()
        {
            "Welcome to our best chat ever:", 
            "----------------------------",
            "Enter your joke now:", 
            "----------------------------",
        };

        //vvv Метод для добавления сообщения в чат
        public static void AddMessage(string message)
        {
            _chatLines.Add(message);
        }

        //vvv Метод отвечающий за вывод (Получения данных) чата
        public static string GetChat()
        {
            return _chatLines
                .Aggregate("", (accumulate, line) => $"{accumulate}\n{line}")
                .TrimStart('\n');
            //^^^ Выполняем общую агрегацию элементов коллекции
        }
    }
}
