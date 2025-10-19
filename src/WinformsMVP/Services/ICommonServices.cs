namespace WinformsMVP.Services
{
    public interface ICommonServices
    {
        IDialogProvider DialogProvider { get; }
        IMessageService MessageService { get; }
        IFileService FileService { get; }
    }
}
