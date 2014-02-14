using Akavache;
using Lager;
using System;

namespace Espera.Android.Settings
{
    public class UserSettings : SettingsStorage
    {
        private static readonly Lazy<UserSettings> instance;

        static UserSettings()
        {
            instance = new Lazy<UserSettings>(() => new UserSettings());
        }

        private UserSettings()
            : base("__Settings__", BlobCache.InMemory)
        { }

        public static UserSettings Instance
        {
            get { return instance.Value; }
        }

        public string AdministratorPassword
        {
            get { return this.GetOrCreate((string)null); }
            set { this.SetOrCreate(value); }
        }

        public DefaultLibraryAction DefaultLibraryAction
        {
            get { return this.GetOrCreate(DefaultLibraryAction.PlayAll); }
            set { this.SetOrCreate(value); }
        }

        public bool EnableAdministratorMode
        {
            get { return this.GetOrCreate(false); }
            set { this.SetOrCreate(value); }
        }

        public int Port
        {
            get { return this.GetOrCreate(49587); }
            set { this.SetOrCreate(value); }
        }
    }
}