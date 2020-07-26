using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LedCSharp;

namespace GHubMute
{
    public class MouseController
    {
        private static bool IsInitialized;

        public static bool Initialize()
        {
            if (!IsInitialized)
            {
                IsInitialized = LogitechGSDK.LogiLedInitWithName(typeof(MouseController).Assembly.GetName().Name);
            }

            return IsInitialized;
        }

        public static void Shutdown()
        {
            if (IsInitialized)
            {
                LogitechGSDK.LogiLedShutdown();
            }
        }

        public int[] Zones { get; set; } = new[] { 0, 1, 2 };

        public int[] IndicatorTimings { get; set; } = new [] { 150, 500 };

        public LogiColor ErrorColor { get; set; } = new LogiColor(100, 0, 89);

        public LogiColor[] CapturingColors { get; set; } = new[]
        {
            new LogiColor(100, 0, 89),
            new LogiColor(100, 0, 0)
        };

        public LogiColor[] MutedColors { get; set; } = new[]
        {
            new LogiColor(100, 100, 0),
            new LogiColor(0, 76, 100)
        };

        public Task Show(AudioCaptureStatus status) => status switch
        {
            AudioCaptureStatus.Muted => ShowMuted(),
            AudioCaptureStatus.Capturing => ShowCapturing(),
            _ => ShowError()
        };

        public async Task ShowMuted()
        {
            await ShowIndicatorColors(MutedColors, IndicatorTimings).ConfigureAwait(false);
        }

        public async Task ShowCapturing()
        {
            await ShowIndicatorColors(CapturingColors, IndicatorTimings).ConfigureAwait(false);
        }

        public async Task ShowError()
        {
            var colors = new[] { ErrorColor, LogiColor.Black };
            await ShowIndicatorColors(Enumerable.Repeat(colors, 4).SelectMany(c => c), new[] { 150 }).ConfigureAwait(false);
        }

        private async Task ShowIndicatorColors(IEnumerable<LogiColor> colors, int[] delays)
        {
            if (!Initialize())
            {
                return;
            }

            var saved = LogitechGSDK.LogiLedSaveCurrentLighting();

            var colorIndex = 0;

            foreach (var color in colors)
            {
                ForceMouseColors(color);
                var delayMs = delays[Math.Min(colorIndex, delays.Length - 1)];
                await Task.Delay(delayMs);
                colorIndex++;
            }

            if (saved)
            {
                LogitechGSDK.LogiLedRestoreLighting();
            }
        }

        private void ForceMouseColors(LogiColor color)
        {
            foreach (var zone in Zones)
            {
                LogitechGSDK.LogiLedSetLightingForTargetZone(DeviceType.Mouse, zone, color.Red, color.Green, color.Blue);
            }
        }
    }
}
