using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reactive;
using System.Threading.Tasks;
using Espera.Mobile.Core.Songs;
using Espera.Network;

namespace Espera.Mobile.Core.Network
{
    public interface INetworkMessenger : IDisposable, INotifyPropertyChanged
    {
        NetworkAccessPermission AccessPermission { get; }

        IObservable<Unit> Disconnected { get; }

        IObservable<GuestSystemInfo> GuestSystemInfoChanged { get; }

        bool IsConnected { get; }

        IObservable<NetworkPlaybackState> PlaybackStateChanged { get; }

        IObservable<TimeSpan> PlaybackTimeChanged { get; }

        IObservable<NetworkPlaylist> PlaylistChanged { get; }

        Task<ResponseInfo> AddSongToPlaylistAsync(Guid songGuid);

        Task<Tuple<ResponseStatus, ConnectionInfo>> ConnectAsync(string address, int port, Guid deviceId, string password);

        Task<ResponseInfo> ContinueSongAsync();

        void Disconnect();

        IObservable<string> DiscoverServerAsync(string localAddress, int port);

        Task<NetworkPlaylist> GetCurrentPlaylistAsync();

        Task<GuestSystemInfo> GetGuestSystemInfo();

        Task<IReadOnlyList<NetworkSong>> GetSongsAsync();

        Task<float> GetVolume();

        Task<ResponseInfo> MovePlaylistSongDownAsync(Guid entryGuid);

        Task<ResponseInfo> MovePlaylistSongUpAsync(Guid entryGuid);

        Task<ResponseInfo> PauseSongAsync();

        Task<ResponseInfo> PlayNextSongAsync();

        Task<ResponseInfo> PlayPlaylistSongAsync(Guid entryGuid);

        Task<ResponseInfo> PlayPreviousSongAsync();

        Task<ResponseInfo> PlaySongsAsync(IEnumerable<Guid> guids);

        Task<FileTransferStatus> QueueRemoteSong(LocalSong songMetadata, byte[] data);

        Task<ResponseInfo> RemovePlaylistSongAsync(Guid entryGuid);

        Task<ResponseInfo> SetCurrentTime(TimeSpan time);

        Task<ResponseInfo> SetVolume(float volume);

        Task<ResponseInfo> VoteAsync(Guid entryGuid);
    }
}