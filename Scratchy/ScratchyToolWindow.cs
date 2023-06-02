using Microsoft.VisualStudio.Shell;
using System.Runtime.InteropServices;

namespace Scratchy
{
    /// <summary>
    /// This class implements the tool window exposed by this package and hosts a user control.
    /// </summary>
    /// <remarks>
    /// In Visual Studio tool windows are composed of a frame (implemented by the shell) and a pane,
    /// usually implemented by the package implementer.
    /// <para>
    /// This class derives from the ToolWindowPane class provided from the MPF in order to use its
    /// implementation of the IVsUIElementPane interface.
    /// </para>
    /// </remarks>
    [Guid("02d2032e-661a-4831-aef3-ebbe904a2c24")]
    public class ScratchyToolWindow : ToolWindowPane
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ScratchyToolWindow"/> class.
        /// </summary>
        public ScratchyToolWindow() : base(null)
        {
            Caption = "Scratchy";

            // This is the user control hosted by the tool window; Note that, even if this class implements IDisposable,
            // we are not calling Dispose on this object. This is because ToolWindowPane calls Dispose on
            // the object returned by the Content property.
            Content = new ScratchyToolWindowControl();
        }
    }
}
