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
            using var deviceEnumerator = new MMDeviceEnumerator();

            int unmuted = 0;
            int total = 0;

            foreach (var item in deviceEnumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active))
            {
                total++;

                using (item)
                {
                    using var volume = item.AudioEndpointVolume;
                    if (!volume.Mute)
                    {
                        unmuted++;
                    }
                }
            }

            if (total == 0)
            {
                return AudioCaptureStatus.Unknown;
            }

            return unmuted == 0
                ? Unmute(deviceEnumerator)
                : Mute(deviceEnumerator);
        }

        public AudioCaptureStatus Mute()
        {
            using var deviceEnumerator = new MMDeviceEnumerator();
            return Mute(deviceEnumerator);
        }

        public AudioCaptureStatus Unmute()
        {
            using var deviceEnumerator = new MMDeviceEnumerator();
            return Unmute(deviceEnumerator);
        }

        private AudioCaptureStatus Mute(MMDeviceEnumerator deviceEnumerator)
        {
            int totalCount = 0;

            foreach (var item in deviceEnumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active))
            {
                totalCount++;

                using (item)
                {
                    using var volume = item.AudioEndpointVolume;
                    if (!volume.Mute)
                    {
                        _state.FlagDeviceAsManaged(item.ID);
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

        private AudioCaptureStatus Unmute(MMDeviceEnumerator deviceEnumerator)
        {
            int totalCount = 0;

            foreach (var item in deviceEnumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active))
            {
                totalCount++;

                using (item)
                {
                    if (_state.DeviceIdIsManaged(item.ID))
                    {
                        using var volume = item.AudioEndpointVolume;
                        if (volume.Mute)
                        {
                            volume.Mute = false;
                        }
                    }
                }
            }

            if (totalCount == 0)
            {
                return AudioCaptureStatus.Unknown;
            }

            return AudioCaptureStatus.Capturing;
        }
    }
}
