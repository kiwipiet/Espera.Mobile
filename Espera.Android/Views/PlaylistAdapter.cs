using Android.App;
using Android.Views;
using Android.Widget;
using Espera.Android.ViewModels;
using ReactiveUI;
using System;
using System.Reactive.Linq;
using Object = Java.Lang.Object;

namespace Espera.Android.Views
{
    internal class PlaylistAdapter : BaseAdapter<PlaylistEntryViewModel>
    {
        private readonly IDisposable changedSubscription;
        private readonly Activity context;
        private readonly IReadOnlyReactiveList<PlaylistEntryViewModel> playlist;

        public PlaylistAdapter(Activity context, IReadOnlyReactiveList<PlaylistEntryViewModel> playlist)
        {
            this.context = context;
            this.playlist = playlist;

            this.changedSubscription = this.playlist.Changed
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => this.NotifyDataSetChanged());
        }

        public override int Count
        {
            get { return this.playlist.Count; }
        }

        public override bool HasStableIds
        {
            get { return true; }
        }

        public override PlaylistEntryViewModel this[int position]
        {
            get { return this.playlist[position]; }
        }

        public override Object GetItem(int position)
        {
            return this.playlist[position].GetHashCode();
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            if (position > this.playlist.Count - 1)
                return convertView ?? context.LayoutInflater.Inflate(Resource.Layout.PlaylistListItem, null);

            View view = convertView ?? context.LayoutInflater.Inflate(Resource.Layout.PlaylistListItem, null);

            PlaylistEntryViewModel entry = this.playlist[position];
            view.FindViewById<TextView>(Resource.Id.PlaylistItemText1).Text = entry.Title;
            view.FindViewById<TextView>(Resource.Id.PlaylistItemText2).Text = entry.Artist;
            view.FindViewById<ImageView>(Resource.Id.Image).Visibility = entry.IsPlaying ? ViewStates.Visible : ViewStates.Gone;

            return view;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            this.changedSubscription.Dispose();
        }
    }
}