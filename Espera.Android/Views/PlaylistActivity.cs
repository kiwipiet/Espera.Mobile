using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Widget;
using Espera.Android.ViewModels;
using ReactiveUI;
using ReactiveUI.Android;
using ReactiveUI.Mobile;
using System;
using System.Reactive.Linq;

namespace Espera.Android.Views
{
    [Activity(Label = "Current Playlist", ConfigurationChanges = ConfigChanges.Orientation)]
    public class PlaylistActivity : ReactiveActivity<PlaylistViewModel>
    {
        private readonly AutoSuspendActivityHelper autoSuspendHelper;
        private ProgressDialog progressDialog;

        public PlaylistActivity()
        {
            this.autoSuspendHelper = new AutoSuspendActivityHelper(this);
        }

        private LinearLayout PlaybackControlPanel
        {
            get { return this.FindViewById<LinearLayout>(Resource.Id.playbackControlPanel); }
        }

        private ListView PlaylistListView
        {
            get { return this.FindViewById<ListView>(Resource.Id.playList); }
        }

        private Button PlayNextSongButton
        {
            get { return this.FindViewById<Button>(Resource.Id.nextButton); }
        }

        private Button PlayPauseButton
        {
            get { return this.FindViewById<Button>(Resource.Id.playPauseButton); }
        }

        private Button PlayPreviousSongButton
        {
            get { return this.FindViewById<Button>(Resource.Id.previousButton); }
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            this.autoSuspendHelper.OnCreate(bundle);

            this.SetContentView(Resource.Layout.Playlist);

            this.ViewModel = new PlaylistViewModel();
            this.ViewModel.Message.Subscribe(x => Toast.MakeText(this, x, ToastLength.Short).Show());

            this.PlaylistListView.Adapter = new PlaylistAdapter(this, this.ViewModel.Entries);
            this.PlaylistListView.ItemClick += (sender, args) =>
            {
                if (this.ViewModel.PlayPlaylistSongCommand.CanExecute(null))
                {
                    this.ViewModel.PlayPlaylistSongCommand.Execute(args.Position);
                }
            };
            Observable.FromEventPattern<AdapterView.ItemLongClickEventArgs>(
                h => this.PlaylistListView.ItemLongClick += h,
                h => this.PlaylistListView.ItemLongClick += h)
                .Select(x => x.EventArgs.Position)
                .CombineLatest(this.ViewModel.CanModify, Tuple.Create)
                .Subscribe(x =>
                {
                    var builder = new AlertDialog.Builder(this);
                    if (x.Item2)
                    {
                        builder.SetItems(new[] { "Play", "Remove", "Move Up", "Move Down", "Vote" }, (o, eventArgs) =>
                        {
                            switch (eventArgs.Which)
                            {
                                case 0:
                                    this.ViewModel.PlayPlaylistSongCommand.Execute(x.Item1);
                                    break;

                                case 1:
                                    this.ViewModel.RemoveSongCommand.Execute(x.Item1);
                                    break;

                                case 2:
                                    this.ViewModel.MoveSongUpCommand.Execute(x.Item1);
                                    break;

                                case 3:
                                    this.ViewModel.MoveSongDownCommand.Execute(x.Item1);
                                    break;

                                case 4:
                                    this.ViewModel.VoteCommand.Execute(x.Item1);
                                    break;
                            }
                        });
                    }

                    else
                    {
                        builder.SetItems(new[] { "Vote" }, (sender, args) => this.ViewModel.VoteCommand.Execute(x.Item1));
                    }

                    builder.Create().Show();
                });
            this.PlaylistListView.EmptyView = this.FindViewById(global::Android.Resource.Id.Empty);

            this.ViewModel.CanModify.Select(x => x ? ViewStates.Visible : ViewStates.Gone)
                .BindTo(this.PlaybackControlPanel, x => x.Visibility);

            this.BindCommand(this.ViewModel, x => x.PlayNextSongCommand, x => x.PlayNextSongButton);
            this.BindCommand(this.ViewModel, x => x.PlayPreviousSongCommand, x => x.PlayPreviousSongButton);
            this.BindCommand(this.ViewModel, x => x.PlayPauseCommand, x => x.PlayPauseButton);

            this.ViewModel.WhenAnyValue(x => x.IsPlaying).Select(x => x ? Resource.Drawable.Pause : Resource.Drawable.Play)
                .Subscribe(x => this.PlayPauseButton.SetBackgroundResource(x));

            Func<bool, int> alphaSelector = x => x ? 255 : 100;

            this.ViewModel.PlayPauseCommand.CanExecuteObservable.Select(alphaSelector)
                .Subscribe(x => this.PlayPauseButton.Background.SetAlpha(x));

            this.ViewModel.PlayPreviousSongCommand.CanExecuteObservable.Select(alphaSelector)
                .Subscribe(x => this.PlayPreviousSongButton.Background.SetAlpha(x));

            this.ViewModel.PlayNextSongCommand.CanExecuteObservable.Select(alphaSelector)
                .Subscribe(x => this.PlayNextSongButton.Background.SetAlpha(x));

            this.progressDialog = new ProgressDialog(this);
            this.progressDialog.SetMessage("Loading playlist");
            this.progressDialog.Indeterminate = true;
            this.progressDialog.SetCancelable(false);

            this.ViewModel.LoadPlaylistCommand.IsExecuting
                .Subscribe(x =>
                {
                    if (x)
                    {
                        this.progressDialog.Show();
                    }

                    else
                    {
                        this.progressDialog.Hide();
                    }
                });

            this.ViewModel.LoadPlaylistCommand.Execute(null);
        }

        protected override void OnPause()
        {
            base.OnPause();
            this.autoSuspendHelper.OnPause();
        }

        protected override void OnResume()
        {
            base.OnResume();
            this.autoSuspendHelper.OnResume();
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);
            this.autoSuspendHelper.OnSaveInstanceState(outState);
        }
    }
}