using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleMessageSender
{
    internal static class ColorConsole
    {
        public static void WriteLine(string value, ConsoleColor color = ConsoleColor.White)
        {
            var currentColor = Console.ForegroundColor;
            try
            {
                Console.ForegroundColor = color;
                Console.WriteLine(value);
            }
            finally
            {
                Console.ForegroundColor = currentColor;
            }
        }
    }
}
