using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

namespace GHubMute
{
    public class PersistentState : IAsyncDisposable
    {
        public static async Task<PersistentState> Load()
        {
            var currentDirectory = Path.GetDirectoryName((Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly()).Location);
            var statePath = Path.Combine(currentDirectory, "ghub-mute-state.json");

            var stream = new FileStream(statePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None, 4096, FileOptions.SequentialScan | FileOptions.Asynchronous);
            var state = new PersistentState(stream);
            await state.Load_Internal();
            return state;
        }

        private PersistentState(FileStream stream)
        {
            _stream = stream;
        }

        private FileStream _stream;
        private bool _requiresSave = false;
        private SerializedState _state;

        public async ValueTask SaveChanges()
        {
            if (!_requiresSave)
            {
                return;
            }

            _stream.Seek(0, SeekOrigin.Begin);
            await JsonSerializer.SerializeAsync(_stream, _state);
            _stream.SetLength(_stream.Position);
        }

        public ValueTask DisposeAsync() => _stream.DisposeAsync();

        public bool DeviceIdIsManaged(string id) => _state.ManagedDeviceIds.Contains(id);

        public void FlagDeviceAsManaged(string id)
        {
            if (!DeviceIdIsManaged(id))
            {
                _state.ManagedDeviceIds.Add(id);
                _requiresSave = true;
            }
        }

        private async ValueTask Load_Internal()
        {
            _stream.Seek(0, SeekOrigin.Begin);
            try
            {
                _state = await JsonSerializer.DeserializeAsync<SerializedState>(_stream);
            }
            catch
            {
                _state = new SerializedState();
            }

            _state.ManagedDeviceIds ??= new List<string>();
        }

        private class SerializedState
        {
            public List<string> ManagedDeviceIds { get; set; }
        }
    }
}
