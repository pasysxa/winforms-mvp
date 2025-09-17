namespace WinformsMVP.Services
{
    public interface ICommonServices
    {
        IDialogService DialogService { get; }
        IMessageService MessageService { get; }
        IFileService FileService { get; }
    }
}
