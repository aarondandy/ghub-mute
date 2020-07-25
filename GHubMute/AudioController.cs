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

        public void Toggle()
        {
            using var deviceEnumerator = new MMDeviceEnumerator();

            int muted = 0;
            int total = 0;

            foreach (var item in deviceEnumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active))
            {
                total++;

                using (item)
                {
                    using var volume = item.AudioEndpointVolume;
                    if (volume.Mute)
                    {
                        muted++;
                    }
                }
            }

            if (total == 0 || muted == 0)
            {
                Mute(deviceEnumerator);
            }
            else
            {
                Unmute(deviceEnumerator);
            }
        }

        public void Mute()
        {
            using var deviceEnumerator = new MMDeviceEnumerator();
            Mute(deviceEnumerator);
        }

        public void Unmute()
        {
            using var deviceEnumerator = new MMDeviceEnumerator();
            Unmute(deviceEnumerator);
        }

        private void Mute(MMDeviceEnumerator deviceEnumerator)
        {
            foreach (var item in deviceEnumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active))
            {
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
        }

        private void Unmute(MMDeviceEnumerator deviceEnumerator)
        {
            foreach (var item in deviceEnumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active))
            {
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
        }
    }
}
