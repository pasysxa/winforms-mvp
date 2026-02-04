using System;
using WinformsMVP.Common.Events;

namespace WinformsMVP.MVP.Presenters
{
    /// <summary>
    /// Interface for presenters that need to control window closing behavior.
    /// Inspired by Prism's IDialogAware but adapted for WinForms MVP pattern.
    /// </summary>
    public interface IRequestClose<TResult>
    {
        /// <summary>
        /// Event raised when the presenter requests the window to close.
        /// The event args contain the result and status of the interaction.
        /// </summary>
        event EventHandler<CloseRequestedEventArgs<TResult>> CloseRequested;

        /// <summary>
        /// Called by the framework to determine if the window can be closed.
        /// Return false to prevent closing (e.g., when there are unsaved changes).
        /// </summary>
        bool CanClose();
    }
}
