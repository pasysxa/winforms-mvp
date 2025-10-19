using System.Drawing.Printing;

namespace WinformsMVP.Services.Implementations.DialogOptions
{
    public class PrintDialogOptions
    {
        public PrinterSettings PrinterSettings { get; set; }
        public bool AllowSelection { get; set; } = true;
        public bool AllowSomePages { get; set; } = true;
    }
}
