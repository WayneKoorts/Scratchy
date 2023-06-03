using System;
using System.Windows.Media;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Events;
using Microsoft.VisualStudio.Shell.Interop;
using SolutionEvents = Microsoft.VisualStudio.Shell.Events.SolutionEvents;

namespace Scratchy
{
    /// <summary>
    /// Interaction logic for ScratchyToolWindowControl.
    /// </summary>
    public partial class ScratchyToolWindowControl
    {
        private const string SolutionClosedPlaceholderText = "Load a solution to start writing notes.";

        private const string SolutionOpenPlaceholderText = "Type your scratchpad notes here and they will be saved automatically.";

        public ScratchyToolWindowControl()
        {
            InitializeComponent();

            VSColorTheme.ThemeChanged += HandleThemeChange;
            SetToolWindowTheme();

            SolutionEvents.OnAfterOpenSolution += HandleSolutionOpened;
            SolutionEvents.OnAfterCloseSolution += HandleSolutionClosed;

            if (SolutionIsOpen())
            {
                EnableScratchyInput();
            }
        }

        private static IVsSolution GetSolution()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var solution = (IVsSolution)Package.GetGlobalService(typeof(SVsSolution));
            return solution;
        }

        private static bool SolutionIsOpen()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            GetSolution().GetProperty((int)__VSPROPID.VSPROPID_IsSolutionOpen, out var isOpen);

            return (bool)isOpen;
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
        
        private void HandleSolutionOpened(object sender, OpenSolutionEventArgs e)
        {
            EnableScratchyInput();
        }

        private void HandleSolutionClosed(object sender, EventArgs e)
        {
            DisableScratchyInput();
        }

        private void EnableScratchyInput()
        {
            ScratchPadTextBox.Text = SolutionOpenPlaceholderText;
            ScratchPadTextBox.IsEnabled = true;
        }

        private void DisableScratchyInput()
        {
            ScratchPadTextBox.Text = SolutionClosedPlaceholderText;
            ScratchPadTextBox.IsEnabled = false;
        }
    }
}
