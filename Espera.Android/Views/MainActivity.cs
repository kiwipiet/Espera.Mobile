﻿using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Net.Wifi;
using Android.OS;
using Android.Views;
using Android.Widget;
using Espera.Android.Services;
using Espera.Mobile.Core.Analytics;
using Espera.Mobile.Core.Network;
using Espera.Mobile.Core.Settings;
using Espera.Mobile.Core.ViewModels;
using Google.Analytics.Tracking;
using ReactiveMarrow;
using ReactiveUI;
using Splat;
using IMenuItem = Android.Views.IMenuItem;

namespace Espera.Android.Views
{
    [Activity(Label = "Espera", MainLauncher = true, Icon = "@drawable/icon",
        ConfigurationChanges = ConfigChanges.Orientation, LaunchMode = LaunchMode.SingleTop)]
    public class MainActivity : ReactiveActivity<MainViewModel>
    {
        public MainActivity()
        {
            var settings = Locator.Current.GetService<UserSettings>();
            var wifiService = Locator.Current.GetService<IWifiService>();

            this.ViewModel = new MainViewModel(settings, wifiService.GetIpAddress);

            this.WhenActivated(() =>
            {
                var disposable = new CompositeDisposable();

                var connectOrDisconnectCommand = this.ViewModel.WhenAnyValue(x => x.IsConnected)
                    .Select(x => x ? (IReactiveCommand)this.ViewModel.DisconnectCommand : this.ViewModel.ConnectCommand);

                connectOrDisconnectCommand.SampleAndCombineLatest(this.ConnectButton.Events().Click, (command, args) => command)
                    .Where(x => x.CanExecute(null))
                    .Subscribe(x => x.Execute(null))
                    .DisposeWith(disposable);

                connectOrDisconnectCommand.SelectMany(x => x.CanExecuteObservable)
                    .BindTo(this.ConnectButton, x => x.Enabled)
                    .DisposeWith(disposable);

                this.ViewModel.ConnectCommand.IsExecuting
                    .CombineLatest(this.ViewModel.WhenAnyValue(x => x.IsConnected), (connecting, connected) =>
                        connected ? "Disconnect" : connecting ? "Connecting..." : "Connect")
                    .BindTo(this.ConnectButton, x => x.Text)
                    .DisposeWith(disposable);

                this.ViewModel.ConnectionFailed
                    .Subscribe(x => Toast.MakeText(this, x, ToastLength.Long).Show())
                    .DisposeWith(disposable);

                this.OneWayBind(this.ViewModel, x => x.IsConnected, x => x.LoadPlaylistButton.Enabled)
                    .DisposeWith(disposable);
                this.LoadPlaylistButton.Events().Click.Subscribe(x => this.StartActivity(typeof(PlaylistActivity)))
                    .DisposeWith(disposable);

                this.OneWayBind(this.ViewModel, x => x.IsConnected, x => x.LoadRemoteArtistsButton.Enabled)
                    .DisposeWith(disposable);
                this.LoadRemoteArtistsButton.Events().Click.Subscribe(x => this.StartActivity(typeof(RemoteArtistsActivity)))
                    .DisposeWith(disposable);

                this.OneWayBind(this.ViewModel, x => x.IsConnected, x => x.LoadLocalArtistsButton.Enabled);
                this.LoadLocalArtistsButton.Events().Click.Subscribe(x => this.StartActivity(typeof(LocalArtistsActivity)))
                    .DisposeWith(disposable); ;

                return disposable;
            });
        }

        public Button ConnectButton { get; private set; }

        public Button LoadLocalArtistsButton { get; private set; }

        public Button LoadPlaylistButton { get; private set; }

        public Button LoadRemoteArtistsButton { get; private set; }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            menu.Add("Settings").SetIcon(Resource.Drawable.Settings)
                .SetShowAsAction(ShowAsAction.Always);

            return true;
        }

        public override bool OnKeyDown(Keycode keyCode, KeyEvent e)
        {
            return AndroidVolumeRequests.Instance.HandleKeyCode(keyCode) || base.OnKeyDown(keyCode, e);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            this.StartActivity(typeof(SettingsActivity));

            return true;
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            this.Title = String.Empty;

            this.SetContentView(Resource.Layout.Main);
            this.WireUpControls();

            this.StartService(new Intent(this, typeof(NetworkService)));
        }

        protected override void OnNewIntent(Intent intent)
        {
            base.OnNewIntent(intent);
            this.Intent = intent;
        }

        protected override void OnResume()
        {
            base.OnResume();

            if (this.Intent.HasExtra(NetworkService.ConnectionLostString))
            {
                Toast.MakeText(this, "Connection lost", ToastLength.Long).Show();
                this.Intent.RemoveExtra(NetworkService.ConnectionLostString);
            }

            var wifiService = Locator.Current.GetService<IWifiService>();

            if (wifiService.GetIpAddress() == null)
            {
                this.ShowWifiPrompt();
            }

            else
            {
                var analytics = Locator.Current.GetService<IAnalytics>();
                analytics.RecordWifiSpeed(wifiService.GetWifiSpeed());
            }
        }

        protected override void OnStart()
        {
            base.OnStart();

            EasyTracker.GetInstance(this).ActivityStart(this);
        }

        protected override void OnStop()
        {
            base.OnStop();

            EasyTracker.GetInstance(this).ActivityStop(this);
        }

        private void ShowWifiPrompt()
        {
            var wifiManager = WifiManager.FromContext(this);
            var builder = new AlertDialog.Builder(this);
            builder.SetTitle("Error");
            builder.SetMessage("You have to enable Wifi.");
            builder.SetPositiveButton("Enable", (sender, args) => wifiManager.SetWifiEnabled(true));
            builder.SetNegativeButton("Exit", (sender, args) => this.Finish());

            builder.Show();
        }
    }
}