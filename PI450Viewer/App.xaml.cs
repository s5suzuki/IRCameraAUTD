using MaterialDesignColors;
using MaterialDesignThemes.Wpf;
using System.Windows;

namespace PI450Viewer
{
    public partial class App 
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            var primaryColor = SwatchHelper.Lookup[MaterialDesignColor.DeepPurple];
            var accentColor = SwatchHelper.Lookup[MaterialDesignColor.Lime];
            var theme = Theme.Create(new MaterialDesignDarkTheme(), primaryColor, accentColor);
            Resources.SetTheme(theme);
            base.OnStartup(e);
        }
    }
}
