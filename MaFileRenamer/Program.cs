using System;

namespace MaFileRenamer
{
    static class Program
    {
        public static void Main()
        {
            Utils.Install();

            try
            {
                Utils.Copyright();
                Renamer.Start();
            }
            catch (Exception ex)
            {
                Utils.Exception(ex.Message);
            }

            Console.ReadKey();
            Environment.Exit(0);
        }
    }
}