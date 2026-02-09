namespace WinformsMVP.Services
{
    /// <summary>
    /// プラットフォームサービスのインターフェース。
    /// メッセージ、ダイアログ、ファイル操作などの基盤サービスを提供します。
    /// </summary>
    public interface IPlatformServices
    {
        /// <summary>
        /// ダイアログプロバイダー（ファイルを開く、保存、フォルダ選択など）
        /// </summary>
        IDialogProvider DialogProvider { get; }

        /// <summary>
        /// メッセージサービス（メッセージボックス、トースト通知など）
        /// </summary>
        IMessageService MessageService { get; }

        /// <summary>
        /// ファイルサービス（ファイル読み書き、ディレクトリ操作など）
        /// </summary>
        IFileService FileService { get; }
    }
}
