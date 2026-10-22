namespace EsrganGui.Models
{
    /// <summary>主题选项。</summary>
    public enum AppTheme
    {
        Light = 0,
        Dark = 1,
        System = 2,
    }

    /// <summary>保存在程序目录 preferences.json 中的首选项。</summary>
    public sealed class AppPreferences
    {
        public AppTheme Theme { get; set; } = AppTheme.Light;

        /// <summary>输出文件名后缀模板，支持 %MODEL%、%SCALE%、%COMPRESS%。</summary>
        public string SuffixTemplate { get; set; } = "%MODEL%";

        /// <summary>是否去掉输出文件名末尾多余的下划线。</summary>
        public bool RemoveTrailingUnderscore { get; set; } = true;

        /// <summary>压缩完成后是否删除引擎生成的未压缩 PNG。</summary>
        public bool DeleteUncompressedOutput { get; set; } = true;

        public AppPreferences Clone() => new()
        {
            Theme = Theme,
            SuffixTemplate = SuffixTemplate,
            RemoveTrailingUnderscore = RemoveTrailingUnderscore,
            DeleteUncompressedOutput = DeleteUncompressedOutput,
        };
    }
}
