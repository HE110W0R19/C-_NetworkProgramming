using System;
using System.Collections.Generic;
using System.Linq;

namespace ChatServer
{
    internal static class ChatDatabase
    {
        private static DateTime nowSystemTime;
        //private static int similar_nicknames = 0;
        private static Dictionary<string, string> _chatLines = new Dictionary<string, string>() { };

        public static void AddMessage(string name, string message)
        {
            if (message.Length != 0)
            {
                nowSystemTime = DateTime.Now;
                string nameAndTime = $"({nowSystemTime.ToLongTimeString()}) {name}";

                //if (_chatLines.ContainsKey(nameAndTime))
                //{
                //    ++similar_nicknames;
                //    name += $"_{similar_nicknames}";
                //}

                _chatLines.Add(nameAndTime, message);
            }
        }

        public static string GetChat()
        {
            return _chatLines
                .Aggregate("", (accumulate, line) => $"{accumulate}\n{line.Key}: {line.Value}")
                .TrimStart('\n');
        }
    }
}
