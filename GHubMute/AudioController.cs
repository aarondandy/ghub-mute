using System.Collections.Generic;
using System.Linq;
using NAudio.CoreAudioApi;

namespace GHubMute
{
    public class AudioController
    {
        public AudioController(PersistentState state)
        {
            _state = state;
        }

        private readonly PersistentState _state;

        public AudioCaptureStatus Toggle()
        {
            var result = CheckInternal();

            if (result.Unmuted.Count != 0)
            {
                return Mute(result.Unmuted);
            }
            else if (result.Muted.Count != 0)
            {
                return Unmute(result.Muted);
            }
            else
            {
                return AudioCaptureStatus.Unknown;
            }
        }

        public AudioCaptureStatus Check()
        {
            var result = CheckInternal();

            if (result.Unmuted.Count != 0)
            {
                return AudioCaptureStatus.Capturing;
            }
            else if (result.Muted.Count != 0)
            {
                return AudioCaptureStatus.Muted;
            }
            else
            {
                return AudioCaptureStatus.Unknown;
            }
        }

        private CheckResult CheckInternal()
        {
            using var deviceEnumerator = new MMDeviceEnumerator();

            var result = new CheckResult();

            foreach (var device in deviceEnumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active))
            {
                using (device)
                {
                    using var volume = device.AudioEndpointVolume;
                    (volume.Mute ? result.Muted : result.Unmuted).Add(device.ID);
                }
            }

            return result;
        }

        public AudioCaptureStatus Mute()
        {
            using var deviceEnumerator = new MMDeviceEnumerator();
            return Mute(deviceEnumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active));
        }

        private AudioCaptureStatus Mute(IEnumerable<string> deviceIds)
        {
            using var deviceEnumerator = new MMDeviceEnumerator();
            return Mute(deviceIds.Select(deviceEnumerator.GetDevice));
        }

        private AudioCaptureStatus Mute(IEnumerable<MMDevice> devices)
        {
            int totalCount = 0;

            foreach (var device in devices)
            {
                totalCount++;

                using (device)
                {
                    using var volume = device.AudioEndpointVolume;
                    if (!volume.Mute)
                    {
                        _state.FlagDeviceAsManaged(device.ID);
                        volume.Mute = true;
                    }
                }
            }

            if (totalCount == 0)
            {
                return AudioCaptureStatus.Unknown;
            }

            return AudioCaptureStatus.Muted;
        }

        public AudioCaptureStatus Unmute()
        {
            using var deviceEnumerator = new MMDeviceEnumerator();
            var devices = deviceEnumerator
                .EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active)
                .Where(d => _state.DeviceIdIsManaged(d.ID));
            return Unmute(devices);
        }

        private AudioCaptureStatus Unmute(IEnumerable<string> deviceIds)
        {
            using var deviceEnumerator = new MMDeviceEnumerator();
            var devices = deviceIds
                .Where(id => _state.DeviceIdIsManaged(id))
                .Select(deviceEnumerator.GetDevice);
            return Unmute(devices);
        }

        private AudioCaptureStatus Unmute(IEnumerable<MMDevice> devices)
        {
            using var deviceEnumerator = new MMDeviceEnumerator();
            int totalCount = 0;

            foreach (var device in devices)
            {
                totalCount++;

                using (device)
                {
                    using var volume = device.AudioEndpointVolume;
                    if (volume.Mute)
                    {
                        volume.Mute = false;
                    }
                }
            }

            if (totalCount == 0)
            {
                return AudioCaptureStatus.Unknown;
            }

            return AudioCaptureStatus.Capturing;
        }

        private class CheckResult
        {
            public List<string> Unmuted = new List<string>();
            public List<string> Muted = new List<string>();
        }
    }
}
