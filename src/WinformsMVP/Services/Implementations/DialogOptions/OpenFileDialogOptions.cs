namespace WinformsMVP.Services.Implementations.DialogOptions
{
    /// <summary>
    /// ファイルを開くダイアログのオプション設定
    /// Titleは指定不要（自動的に日本語デフォルト値が使用されます）
    /// </summary>
    public class OpenFileDialogOptions
    {
        /// <summary>
        /// ファイルフィルター（例: "テキストファイル|*.txt|すべてのファイル|*.*"）
        /// </summary>
        public string Filter { get; set; }

        /// <summary>
        /// 初期ディレクトリ
        /// </summary>
        public string InitialDirectory { get; set; }

        /// <summary>
        /// デフォルトファイル名
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// ファイルが存在しない場合に警告を表示するか
        /// </summary>
        public bool CheckFileExists { get; set; } = true;

        /// <summary>
        /// パスが存在しない場合に警告を表示するか
        /// </summary>
        public bool CheckPathExists { get; set; } = true;

        /// <summary>
        /// タイトル（通常は指定不要、nullの場合はDialogDefaults.OpenFileDialogTitleが使用されます）
        /// </summary>
        public string Title { get; set; }
    }
}
