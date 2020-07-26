using System;
using System.CommandLine;
using System.CommandLine.IO;
using System.Runtime.InteropServices;

namespace GHubMute
{
    class WinConsole : IConsole
    {
        static WinConsole()
        {
            ConsoleAttached = false;
        }

        private static bool ConsoleAttached;

        public static void Setup()
        {
            if (!ConsoleAttached)
            {
                ConsoleAttached = AttachConsole(-1);
            }
        }

        public static void Cleanup()
        {
            if (ConsoleAttached)
            {
                FreeConsole();
                ConsoleAttached = false;
            }
        }

        [DllImport("kernel32.dll")]
        static extern bool AttachConsole(int dwProcessId);

        [DllImport("kernel32.dll")]
        static extern bool AllocConsole();

        [DllImport("kernel32.dll")]
        static extern bool FreeConsole();

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        public WinConsole()
        {
            _wrappedConsole = new Lazy<IConsole>(() =>
            {
                Setup();
                return ConsoleAttached switch
                {
                    true => new SystemConsole(),
                    _ => new TestConsole()
                };
            });
        }

        private readonly Lazy<IConsole> _wrappedConsole;

        public IStandardStreamWriter Out => _wrappedConsole.Value.Out;

        public bool IsOutputRedirected => _wrappedConsole.Value.IsOutputRedirected;

        public IStandardStreamWriter Error => _wrappedConsole.Value.Error;

        public bool IsErrorRedirected => _wrappedConsole.Value.IsErrorRedirected;

        public bool IsInputRedirected => _wrappedConsole.Value.IsInputRedirected;
    }
}
