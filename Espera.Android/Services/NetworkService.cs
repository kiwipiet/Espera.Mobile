using System;
using System.Reactive.Linq;
using Android.App;
using Android.Content;
using Android.OS;
using Espera.Android.Analytics;
using Espera.Android.Views;
using Espera.Mobile.Core.Network;
using Espera.Mobile.Core.Settings;
using ReactiveMarrow;
using ReactiveUI;

namespace Espera.Android.Services
{
    [Service]
    internal class NetworkService : Service
    {
        private INetworkMessenger keepAlive;

        public override IBinder OnBind(Intent intent)
        {
            return null;
        }

        public override void OnCreate()
        {
            base.OnCreate();

            NetworkMessenger.Instance.RegisterAnalytics(new AndroidAnalytics(this.ApplicationContext));

            this.keepAlive = NetworkMessenger.Instance;
            this.keepAlive.IsConnected.Where(x => x)
                .Subscribe(x => this.NotifyNetworkMessengerConnected());

            this.keepAlive.Disconnected.Subscribe(x => this.NotifyNetworkMessengerDisconnected());

            NetworkMessenger.Instance.IsConnected.SampleAndCombineLatest(UserSettings.Instance
                .WhenAnyValue(x => x.Port).DistinctUntilChanged(), (connected, _) => connected)
                .Where(x => x)
                .Subscribe(x => NetworkMessenger.Instance.Disconnect());
        }

        public override void OnDestroy()
        {
            keepAlive.Disconnect();
            keepAlive.Dispose();

            base.OnDestroy();
        }

        private void NotifyNetworkMessengerConnected()
        {
            var notification = new Notification(Resource.Drawable.Play, "Espera is connected");
            var intent = new Intent(this, typeof(MainActivity)).SetAction(Intent.ActionMain).AddCategory(Intent.CategoryLauncher);
            var pendingIntent = PendingIntent.GetActivity(this, 0, intent, 0);
            notification.SetLatestEventInfo(this, "Espera Network", "Espera is connected", pendingIntent);

            this.StartForeground((int)NotificationFlags.ForegroundService, notification);
        }

        private void NotifyNetworkMessengerDisconnected()
        {
            this.StopForeground(true);
        }
    }
}