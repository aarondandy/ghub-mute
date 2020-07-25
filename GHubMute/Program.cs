using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace GHubMute
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            var environmentName = Environment.GetEnvironmentVariable("NETCORE_ENVIRONMENT") ?? "Production";
            var configuaration = new ConfigurationBuilder()
                .AddJsonFile($"appsettings.json", optional: true)
                .AddJsonFile($"appsettings.{environmentName}.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            await using var state = await PersistentState.Load();
            var audioController = new AudioController(state);

            var toggleCommand = new Command("toggle", "Toggles the mute state")
            {
                Handler = CommandHandler.Create(audioController.Toggle)
            };
            var muteCommand = new Command("mute", "Forces the input devices to be muted")
            {
                Handler = CommandHandler.Create(audioController.Mute)
            };
            var unmuteCommand = new Command("unmute", "Forces the muted input devices to become unmuted")
            {
                Handler = CommandHandler.Create(audioController.Unmute)
            };

            var rootCommand = new RootCommand
            {
                toggleCommand,
                muteCommand,
                unmuteCommand
            };
            rootCommand.Description = "Works with G-Hub to control microphone inputs";
            rootCommand.Handler = toggleCommand.Handler;

            var result = await rootCommand.InvokeAsync(args);

            await state.SaveChanges();

            return result;

        }
    }
}
