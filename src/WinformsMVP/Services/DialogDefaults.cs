namespace WinformsMVP.Services
{
    /// <summary>
    /// ダイアログプロバイダーのデフォルト値を提供します。
    /// これらの値はアプリケーション起動時にカスタマイズできます。
    /// </summary>
    public static class DialogDefaults
    {
        #region File Dialog Defaults

        /// <summary>
        /// ファイルを開くダイアログのデフォルトタイトル
        /// </summary>
        public static string OpenFileDialogTitle { get; set; } = "ファイルを開く";

        /// <summary>
        /// ファイルを保存ダイアログのデフォルトタイトル
        /// </summary>
        public static string SaveFileDialogTitle { get; set; } = "ファイルを保存";

        /// <summary>
        /// フォルダ選択ダイアログのデフォルトタイトル
        /// </summary>
        public static string FolderBrowserDialogTitle { get; set; } = "フォルダを選択";

        /// <summary>
        /// ファイルダイアログのデフォルトフィルター
        /// </summary>
        public static string DefaultFileFilter { get; set; } = "すべてのファイル (*.*)|*.*";

        /// <summary>
        /// フォルダ選択ダイアログのデフォルト説明文
        /// </summary>
        public static string FolderBrowserDescription { get; set; } = "フォルダを選択してください";

        #endregion

        #region Print Dialog Defaults

        /// <summary>
        /// 印刷プレビューダイアログのデフォルトタイトル
        /// </summary>
        public static string PrintPreviewDialogTitle { get; set; } = "印刷プレビュー";

        #endregion

        #region Message Dialog Defaults

        /// <summary>
        /// メッセージボックスのデフォルトキャプション
        /// </summary>
        public static string DefaultMessageCaption { get; set; } = "システム";

        #endregion

        #region Error Messages

        /// <summary>
        /// ファイルを開く操作が失敗した時のエラーメッセージ
        /// </summary>
        public static string FileOpenErrorMessage { get; set; } = "ファイルを開くに失敗しました";

        /// <summary>
        /// ファイルを保存操作が失敗した時のエラーメッセージ
        /// </summary>
        public static string FileSaveErrorMessage { get; set; } = "ファイルの保存に失敗しました";

        /// <summary>
        /// フォルダ選択が失敗した時のエラーメッセージ
        /// </summary>
        public static string FolderSelectErrorMessage { get; set; } = "フォルダ選択に失敗しました";

        /// <summary>
        /// 色選択が失敗した時のエラーメッセージ
        /// </summary>
        public static string ColorSelectErrorMessage { get; set; } = "色選択に失敗しました";

        /// <summary>
        /// フォント選択が失敗した時のエラーメッセージ
        /// </summary>
        public static string FontSelectErrorMessage { get; set; } = "フォント選択に失敗しました";

        /// <summary>
        /// 印刷ダイアログが失敗した時のエラーメッセージ
        /// </summary>
        public static string PrintDialogErrorMessage { get; set; } = "印刷ダイアログに失敗しました";

        /// <summary>
        /// ページ設定が失敗した時のエラーメッセージ
        /// </summary>
        public static string PageSetupErrorMessage { get; set; } = "ページ設定に失敗しました";

        /// <summary>
        /// 印刷プレビューが失敗した時のエラーメッセージ
        /// </summary>
        public static string PrintPreviewErrorMessage { get; set; } = "印刷プレビューに失敗しました";

        #endregion
    }
}
