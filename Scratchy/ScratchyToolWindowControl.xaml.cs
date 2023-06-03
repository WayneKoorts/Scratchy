using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Media;
using EnvDTE;
using Microsoft.VisualStudio;
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
        private const string ScratchyKey = "Scratchy";
        private const string SolutionClosedPlaceholderText = "Load a solution to start writing notes.";
        private const string SolutionOpenPlaceholderText = "Type your scratchpad notes here and they will be saved when you close the solution.";

        private ScratchyPackage _scratchy;

        public ScratchyToolWindowControl()
        {
            InitializeComponent();

            ThreadHelper.ThrowIfNotOnUIThread();
            var vsShell = (IVsShell)ServiceProvider.GlobalProvider.GetService(typeof(IVsShell));
            var packageGuidString = Guid.Parse("4bac9d00-91ec-4832-b437-84f7e69bb5c2");
            if (vsShell.IsPackageLoaded(ref packageGuidString, out var scratchyPackage) == VSConstants.S_OK)
            {
                _scratchy = (ScratchyPackage)scratchyPackage;
            }

            VSColorTheme.ThemeChanged += HandleThemeChange;
            SetToolWindowTheme();

            SolutionEvents.OnAfterOpenSolution += HandleSolutionOpened;
            SolutionEvents.OnBeforeCloseSolution += HandleSolutionAboutToClose;
            SolutionEvents.OnAfterCloseSolution += HandleSolutionClosed;

            if (SolutionIsOpen())
            {
                _scratchy.JoinableTaskFactory.Run(RetrieveSavedSolutionNotesAsync);
                EnableScratchyInput();
            }
        }

        private async Task RetrieveSavedSolutionNotesAsync()
        {
            var solution = await GetDteSolutionAsync();
            if (solution == null)
            {
                Debug.WriteLine("Unable to get DTE solution.");
                return;
            }

            Debug.WriteLine($"Scratchy: Solution name: {solution.FullName}");
            var savedNotes = _scratchy.SettingsStore.GetString(ScratchyKey, solution.FullName, null);
            if (!string.IsNullOrEmpty(savedNotes))
            {
                ScratchPadTextBox.Text = savedNotes;
            }
        }

        private async Task SaveSolutionNotesAsync()
        {
            if (string.Equals(ScratchPadTextBox.Text, SolutionOpenPlaceholderText, StringComparison.InvariantCultureIgnoreCase)
                || string.Equals(ScratchPadTextBox.Text, SolutionClosedPlaceholderText, StringComparison.InvariantCultureIgnoreCase))
            {
                return;
            }

            var solution = await GetDteSolutionAsync();
            if (solution == null)
            {
                Debug.WriteLine("Unable to get DTE solution.");
                return;
            }

            Debug.WriteLine($"Scratchy: Solution name: {solution.FullName}");
            
            await _scratchy.JoinableTaskFactory.SwitchToMainThreadAsync();
            if (!_scratchy.SettingsStore.CollectionExists(ScratchyKey))
            {
                _scratchy.SettingsStore.CreateCollection(ScratchyKey);
            }
            _scratchy.SettingsStore.SetString(ScratchyKey, solution.FullName, ScratchPadTextBox.Text);
        }

        private static async Task<Solution> GetDteSolutionAsync()
        {
            var dte = (DTE) await ServiceProvider.GetGlobalServiceAsync(typeof(DTE));
            var solution = dte.Solution;

            return solution;
        }

        private static IVsSolution GetIVsSolution()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var solution = (IVsSolution)Package.GetGlobalService(typeof(SVsSolution));
            return solution;
        }

        private static bool SolutionIsOpen()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            GetIVsSolution().GetProperty((int)__VSPROPID.VSPROPID_IsSolutionOpen, out var isOpen);

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
            _scratchy.JoinableTaskFactory.Run(RetrieveSavedSolutionNotesAsync);
        }

        private void HandleSolutionAboutToClose(object sender, EventArgs e)
        {
            _scratchy.JoinableTaskFactory.Run(SaveSolutionNotesAsync);
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
