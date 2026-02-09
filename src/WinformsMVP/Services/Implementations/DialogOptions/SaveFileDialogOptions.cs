namespace WinformsMVP.Services.Implementations.DialogOptions
{
    /// <summary>
    /// ファイルを保存ダイアログのオプション設定
    /// Titleは指定不要（自動的に日本語デフォルト値が使用されます）
    /// </summary>
    public class SaveFileDialogOptions
    {
        /// <summary>
        /// ファイルフィルター（例: "テキストファイル|*.txt|すべてのファイル|*.*"）
        /// </summary>
        public string Filter { get; set; }

        /// <summary>
        /// デフォルト拡張子（例: "txt"）
        /// </summary>
        public string DefaultExt { get; set; }

        /// <summary>
        /// 初期ディレクトリ
        /// </summary>
        public string InitialDirectory { get; set; }

        /// <summary>
        /// デフォルトファイル名
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// 上書き確認を表示するか
        /// </summary>
        public bool OverwritePrompt { get; set; } = true;

        /// <summary>
        /// パスが存在しない場合に自動作成するか
        /// </summary>
        public bool CreatePrompt { get; set; } = false;

        /// <summary>
        /// タイトル（通常は指定不要、nullの場合はDialogDefaults.SaveFileDialogTitleが使用されます）
        /// </summary>
        public string Title { get; set; }
    }
}
