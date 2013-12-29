﻿using Espera.Android.Network;
using Espera.Android.ViewModels;
using Microsoft.Reactive.Testing;
using Moq;
using ReactiveUI;
using ReactiveUI.Testing;
using System;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Xunit;

namespace Espera.Android.Tests
{
    public class PlaylistViewModelTest
    {
        [Fact]
        public void LoadPlaylistCommandHasTimeout()
        {
            var messenger = CreateDefaultPlaylistMessenger();
            messenger.Setup(x => x.GetCurrentPlaylist()).Returns(async () =>
            {
                await Task.Delay(1000);
                return null;
            });

            var vm = new PlaylistViewModel();

            var thrown = vm.LoadPlaylistCommand.ThrownExceptions.CreateCollection();

            (new TestScheduler()).With(scheduler =>
            {
                vm.LoadPlaylistCommand.Execute(null);
                scheduler.AdvanceByMs(15000);
            });

            Assert.Equal(1, thrown.Count);
        }

        [Fact]
        public void LoadPlaylistCommandSmokeTest()
        {
            var songs = Helpers.SetupSongs(2);
            var playlist = new Playlist("Playlist 1", songs, 1);

            var messenger = CreateDefaultPlaylistMessenger();
            messenger.Setup(x => x.GetCurrentPlaylist()).Returns(playlist.ToTaskResult);

            var vm = new PlaylistViewModel();

            vm.LoadPlaylistCommand.Execute(null);

            Assert.Equal(playlist, vm.Playlist);
        }

        [Fact]
        public void PlaylistChangeUpdatesPlaylist()
        {
            var songs = Helpers.SetupSongs(2);
            var playlist = new Playlist("Playlist 1", songs, 1);

            var messenger = CreateDefaultPlaylistMessenger();
            messenger.SetupGet(x => x.PlaylistChanged).Returns(Observable.Return(playlist));

            var vm = new PlaylistViewModel();

            Assert.Equal(playlist.CurrentIndex, vm.Playlist.CurrentIndex);
            Assert.Equal(playlist.Name, vm.Playlist.Name);
            Assert.True(playlist.Songs.SequenceEqual(vm.Playlist.Songs));
        }

        [Fact]
        public void PlaylistIndexChangeUpdatesPlaylist()
        {
            var songs = Helpers.SetupSongs(2);
            var playlist = new Playlist("Playlist 1", songs, 0);

            var messenger = CreateDefaultPlaylistMessenger();
            messenger.Setup(x => x.GetCurrentPlaylist()).Returns(playlist.ToTaskResult);

            var index = new Subject<int?>();
            messenger.SetupGet(x => x.PlaylistIndexChanged).Returns(index);

            var vm = new PlaylistViewModel();

            vm.LoadPlaylistCommand.Execute(null);

            index.OnNext(1);

            Assert.Equal(1, vm.Playlist.CurrentIndex);
        }

        [Fact]
        public void PlayNextSongCommandSmokeTest()
        {
            var songs = Helpers.SetupSongs(2);
            var playlist = new Playlist("Playlist 1", songs, 0);

            var messenger = CreateDefaultPlaylistMessenger();
            messenger.Setup(x => x.GetCurrentPlaylist()).Returns(playlist.ToTaskResult());
            messenger.Setup(x => x.PlayNextSong()).Returns(new ResponseInfo(200, "Ok").ToTaskResult());

            var index = new Subject<int?>();
            messenger.SetupGet(x => x.PlaylistIndexChanged).Returns(index);

            var vm = new PlaylistViewModel();
            vm.LoadPlaylistCommand.Execute(null);

            var canExecute = vm.PlayNextSongCommand.CanExecuteObservable.CreateCollection();

            vm.PlayNextSongCommand.Execute(null);

            index.OnNext(1);
            index.OnNext(0);
            index.OnNext(null);

            messenger.Verify(x => x.PlayNextSong(), Times.Once);
            Assert.Equal(new[] { true, false, true, false, true, false }, canExecute);
        }

        [Fact]
        public void PlayPauseCommandSmokeTest()
        {
            var songs = Helpers.SetupSongs(2);
            var playlist = new Playlist("Playlist 1", songs, 0);

            var messenger = CreateDefaultPlaylistMessenger();
            messenger.Setup(x => x.GetCurrentPlaylist()).Returns(playlist.ToTaskResult());
            messenger.Setup(x => x.ContinueSong()).Returns(new ResponseInfo(200, "Ok").ToTaskResult());
            messenger.Setup(x => x.PauseSong()).Returns(new ResponseInfo(200, "Ok").ToTaskResult());

            var playbackState = new Subject<PlaybackState>();
            messenger.SetupGet(x => x.PlaybackStateChanged).Returns(playbackState);

            var vm = new PlaylistViewModel();
            vm.LoadPlaylistCommand.Execute(null);

            var canExecute = vm.PlayPauseCommand.CanExecuteObservable.CreateCollection();

            playbackState.OnNext(PlaybackState.Paused);

            vm.PlayPauseCommand.Execute(null);
            messenger.Verify(x => x.ContinueSong(), Times.Once);

            playbackState.OnNext(PlaybackState.Playing);

            vm.PlayPauseCommand.Execute(null);
            messenger.Verify(x => x.PauseSong(), Times.Once);

            playbackState.OnNext(PlaybackState.Paused);

            Assert.Equal(new[] { false, true, false, true, false, true }, canExecute);
        }

        [Fact]
        public void PlayPlaylistSongCommandSmokeTest()
        {
            var songs = Helpers.SetupSongs(2);
            var playlist = new Playlist("Playlist 1", songs, 0);

            var messenger = CreateDefaultPlaylistMessenger();
            messenger.Setup(x => x.GetCurrentPlaylist()).Returns(playlist.ToTaskResult());
            messenger.Setup(x => x.PlayPlaylistSong(It.Is<Guid>(y => y == songs[1].Guid)))
                .Returns(new ResponseInfo(200, "Ok").ToTaskResult());

            var vm = new PlaylistViewModel();

            var coll = vm.Message.CreateCollection();

            vm.LoadPlaylistCommand.Execute(null);

            vm.PlayPlaylistSongCommand.Execute(1);

            messenger.Verify();
            messenger.Verify(x => x.PlayPlaylistSong(It.IsAny<Guid>()), Times.Once);
            Assert.Equal(1, coll.Count);
        }

        [Fact]
        public void PlayPreviousSongCommandSmokeTest()
        {
            var songs = Helpers.SetupSongs(2);
            var playlist = new Playlist("Playlist 1", songs, 1);

            var messenger = CreateDefaultPlaylistMessenger();
            messenger.Setup(x => x.GetCurrentPlaylist()).Returns(playlist.ToTaskResult());
            messenger.Setup(x => x.PlayPreviousSong()).Returns(new ResponseInfo(200, "Ok").ToTaskResult());

            var index = new Subject<int?>();
            messenger.SetupGet(x => x.PlaylistIndexChanged).Returns(index);

            var vm = new PlaylistViewModel();
            vm.LoadPlaylistCommand.Execute(null);

            var canExecute = vm.PlayPreviousSongCommand.CanExecuteObservable.CreateCollection();

            vm.PlayPreviousSongCommand.Execute(null);

            index.OnNext(0);
            index.OnNext(1);
            index.OnNext(null);

            messenger.Verify(x => x.PlayPreviousSong(), Times.Once);
            Assert.Equal(new[] { true, false, true, false, true, false }, canExecute);
        }

        private static Mock<INetworkMessenger> CreateDefaultPlaylistMessenger()
        {
            var messenger = new Mock<INetworkMessenger>();
            messenger.SetupGet(x => x.PlaybackStateChanged).Returns(Observable.Never<PlaybackState>());
            messenger.SetupGet(x => x.PlaylistChanged).Returns(Observable.Never<Playlist>());
            messenger.SetupGet(x => x.PlaylistIndexChanged).Returns(Observable.Never<int?>());
            messenger.SetupGet(x => x.AccessPermission).Returns(Observable.Return(AccessPermission.Admin));
            messenger.Setup(x => x.GetPlaybackState()).Returns(PlaybackState.None.ToTaskResult());

            NetworkMessenger.Override(messenger.Object);

            return messenger;
        }
    }
}