using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Threading.Tasks;

namespace GHubMute
{
    class Program
    {
        [STAThread]
        static async Task<int> Main(string[] args)
        {
            using var state = await PersistentState.LoadAsync();
            var audioController = new AudioController(state);
            AudioCaptureStatus? status = null;

            var capturingColors = new Option<string>(
                new[] { "--cc", "--capture-colors" },
                getDefaultValue: () => "50,0,25;100,0,10",
                description: "Mouse colors when mics are capturing audio"
            );

            var muteColors = new Option<string>(
                new[] { "--mc", "--mute-colors" },
                getDefaultValue: () => "100,100,0;0,40,60",
                description: "Mouse colors when mics are muted"
            );

            var toggleCommand = new Command("toggle", "Toggles the mute state")
            {
                Handler = CommandHandler.Create(() =>
                {
                    status = audioController.Toggle();
                })
            };
            var muteCommand = new Command("mute", "Forces the input devices to be muted")
            {
                Handler = CommandHandler.Create(() =>
                {
                    status = audioController.Mute();
                })
            };
            var unmuteCommand = new Command("unmute", "Forces the muted input devices to become unmuted")
            {
                Handler = CommandHandler.Create(() =>
                {
                    status = audioController.Unmute();
                })
            };
            var checkCommand = new Command("check", "Checks the mute status of audio devices")
            {
                Handler = CommandHandler.Create(() =>
                {
                    status = audioController.Check();
                })
            };

            var rootCommand = new RootCommand
            {
                toggleCommand,
                muteCommand,
                unmuteCommand,
                checkCommand,
                capturingColors,
                muteColors
            };
            rootCommand.Description = "Works with G-Hub to control microphone inputs";
            rootCommand.Handler = toggleCommand.Handler;

            try
            {
                var invokeResult = rootCommand.Invoke(args, new WinConsole());

                var saveTask = state.SaveChangesAsync();

                if (status.HasValue)
                {
                    var mouseController = new MouseController();

                    var parseResult = rootCommand.Parse(args);
                    mouseController.CapturingColors = LogiColor.ParseMultiple(parseResult.ValueForOption(capturingColors));
                    mouseController.MutedColors = LogiColor.ParseMultiple(parseResult.ValueForOption(muteColors));
                    mouseController.Show(status.GetValueOrDefault());
                }

                await saveTask;

                return invokeResult;
            }
            finally
            {
                MouseController.Shutdown();
                WinConsole.Cleanup();
            }
        }
    }
}
