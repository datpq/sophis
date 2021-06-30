using ExpressoReporting.DataModel;
using Newtonsoft.Json;
using Plugin.Settings;
using Plugin.Settings.Abstractions;

namespace ExpressoReporting
{
    public static class Settings
    {
        private static ISettings AppSettings => CrossSettings.Current;

        public static bool IsUserLoggedIn
        {
            get => AppSettings.GetValueOrDefault(nameof(IsUserLoggedIn), false);
            set => AppSettings.AddOrUpdateValue(nameof(IsUserLoggedIn), value);
        }

        public static User LastUser
        {
            get => JsonConvert.DeserializeObject<User>(AppSettings.GetValueOrDefault(nameof(LastUser), string.Empty));
            set => AppSettings.AddOrUpdateValue(nameof(LastUser), JsonConvert.SerializeObject(value));
        }
    }
}
