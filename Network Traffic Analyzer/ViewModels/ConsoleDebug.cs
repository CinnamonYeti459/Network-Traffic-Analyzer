using System.Runtime.InteropServices;

namespace Network_Traffic_Analyzer.ViewModels
{
    public static class ConsoleDebug
    {
        [DllImport("kernel32.dll")]
        private static extern bool AllocConsole();

        public static void ShowConsole()
        {
            AllocConsole();
        }
    }
}
