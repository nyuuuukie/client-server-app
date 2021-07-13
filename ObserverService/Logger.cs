using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObserverService
{
    public static class Logger
    {
        public static void WriteStatus( string status, 
                                        ConsoleColor color = ConsoleColor.DarkMagenta)
        {
            Console.ForegroundColor = color;
            Console.Write(status);
            Console.ResetColor();
            Console.Write(" | ");
        }

        public static void Write(string message,
            ConsoleColor fc = ConsoleColor.White,
            ConsoleColor bc = ConsoleColor.Black)
        {
            Console.ForegroundColor = fc;
            Console.BackgroundColor = bc;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        public static void WriteInfo(string message)
        {
            WriteStatus("INFO", ConsoleColor.DarkYellow);
            Console.WriteLine(message);
            Console.WriteLine();
        }

        public static void WriteError(string message)
        {
            WriteStatus("ERROR", ConsoleColor.DarkRed);
            Console.WriteLine(message);
            Console.WriteLine();
        }

        public static void WriteSuccess(string message)
        {
            WriteStatus("SUCCESS", ConsoleColor.Green);
            Console.WriteLine(message);
            Console.WriteLine();
        }
    }
}
