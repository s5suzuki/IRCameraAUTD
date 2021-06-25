using System;
using System.IO;
using MaterialDesignColors;
using MaterialDesignThemes.Wpf;
using System.Windows;
using PI450Viewer.Models;
using Theme = MaterialDesignThemes.Wpf.Theme;

namespace PI450Viewer
{
    public partial class App
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                if (!File.Exists("settings.json"))
                    SettingManager.SaveSetting("settings.json");
                SettingManager.LoadSetting("settings.json");
                var primaryColor = SwatchHelper.Lookup[MaterialDesignColor.DeepPurple];
                var accentColor = SwatchHelper.Lookup[MaterialDesignColor.Lime];
                var theme = General.Instance.BaseThemeStore == Models.Theme.Dark ? Theme.Create(new MaterialDesignDarkTheme(), primaryColor, accentColor) : Theme.Create(new MaterialDesignLightTheme(), primaryColor, accentColor);
                Resources.SetTheme(theme);
                base.OnStartup(e);
            }
            catch (Exception)
            {
                // ignore
            }
        }
    }
}
