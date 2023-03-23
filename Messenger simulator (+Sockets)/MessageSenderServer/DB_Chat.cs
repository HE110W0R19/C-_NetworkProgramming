using System.Collections.Generic;
using System.Linq;

namespace MessageSenderServer
{
    internal class DB_Chat
    {
        private static List<string> _ChatMessages = new List<string>() { "Hi user 1", "Hi user 2" };
        public static void AddMessage(string mess)
        {
            _ChatMessages.Add(mess);
        }
        public static void RemoveMessage(string mess)
        {
            _ChatMessages.Remove(mess);
        }
        public static string getChat()
        {
            return _ChatMessages.Aggregate("", (accumulate, line) => $"{accumulate} \n {line}");
        }
    }
}
