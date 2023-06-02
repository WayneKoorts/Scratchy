using System.Windows.Media;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;

namespace Scratchy
{
    /// <summary>
    /// Interaction logic for ScratchyToolWindowControl.
    /// </summary>
    public partial class ScratchyToolWindowControl
    {
        public ScratchyToolWindowControl()
        {
            InitializeComponent();
            
            VSColorTheme.ThemeChanged += HandleThemeChange;
            SetToolWindowTheme();
        }

        private void SetToolWindowTheme()
        {
            ScratchPadTextBox.Background = new SolidColorBrush(GetColorFromVsTheme(EnvironmentColors.ToolWindowBackgroundColorKey));
            ScratchPadTextBox.Foreground = new SolidColorBrush(GetColorFromVsTheme(EnvironmentColors.ToolWindowTextColorKey));
        }

        /// <summary>
        /// Get a color from the current Visual Studio theme in a format appropriate
        /// for WPF brushes.
        /// </summary>
        /// <param name="colorKey">A value from the <see cref="EnvironmentColors" /> class.</param>
        /// <returns></returns>
        private static Color GetColorFromVsTheme(ThemeResourceKey colorKey)
        {
            var themeColour = VSColorTheme.GetThemedColor(colorKey);
            return Color.FromRgb(themeColour.R, themeColour.G, themeColour.B);
        }

        private void HandleThemeChange(ThemeChangedEventArgs e)
        {
            SetToolWindowTheme();
        }
    }

}
