using System;

namespace CSVBreakdownConsolidator
{
    public sealed class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length == 0 || args[0].ToLower() == "help")
                Console.WriteLine(CSVConsolidator.HELP_MESSAGE);
            else
            {
                try
                {
                    CSVConsolidator.Execute(args);
                    Console.WriteLine($"Consolidated successfully to {CSVConsolidator.OUTPUT_CSV_NAME}.");
                }
                catch(Exception e){ Console.WriteLine($"Error: {e.Message}"); }
            }
        }
    }
}
