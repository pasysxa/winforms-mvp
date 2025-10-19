namespace WinformsMVP.Services.Implementations.DialogOptions
{
    public class SaveFileDialogOptions
    {
        public string Filter { get; set; } = "All files (*.*)|*.*";
        public string DefaultExt { get; set; } = "";
        public string InitialDirectory { get; set; } = "";
        public string Title { get; set; } = "Save File";
    }
}
