using System;

namespace EveIntelBuddy
{
    public static class Utility
    {
        public static string ConsolePrompt(string prompt)
        {
            var result = "";
            while (string.IsNullOrEmpty(result))
            {
                Console.Out.WriteLine(prompt);
                result = Console.ReadLine();
            }

            Console.Out.WriteLine("");
            return result;
        }
    }
}