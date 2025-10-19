namespace WinformsMVP.Services.Implementations.DialogOptions
{
    public class OpenFileDialogOptions
    {
        public string Filter { get; set; } = "All files (*.*)|*.*";
        public bool Multiselect { get; set; } = false;
        public string InitialDirectory { get; set; } = "";
        public string Title { get; set; } = "Open File";
    }
}
