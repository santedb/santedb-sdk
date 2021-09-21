using System;

namespace PakMan
{
    public static class Emit
    {

        /// <summary>
        /// Emit message
        /// </summary>
        public static void Message(String category, String message, params object[] args)
        {
            switch (category)
            {
                case "INFO":
                    Console.ForegroundColor = ConsoleColor.Green;
                    break;
                case "WARN":
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case "ERROR":
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
            }

            if (args.Length > 0)
                Console.WriteLine("{0}: {1}", category, String.Format(message ?? "", args));
            else
                Console.WriteLine("{0}: {1}", category, message);
            Console.ResetColor();

        }
    }
}
