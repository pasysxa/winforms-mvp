using System.Windows.Forms;
using WinformsMVP.MVP.Views;
using WinformsMVP.Services;

namespace WinformsMVP.Common.Helpers
{
    public class ConfirmationHelper
    {
        public static CommitResult ConfirmAndCommit(IValidationCommitable commitable, IMessageService messageService, string message = "")
        {
            if (!commitable.HasChanges())
                return CommitResult.Skip;

            if (!commitable.Validate())
                return CommitResult.Cancel;

            var result = messageService.ConfirmYesNoCancel(message);

            switch (result)
            {
                case DialogResult.Yes:
                    commitable.Commit();
                    return CommitResult.Commit;
                case DialogResult.No:
                    return CommitResult.Skip;
                default:
                    return CommitResult.Cancel;
            }
        }
    }
}
