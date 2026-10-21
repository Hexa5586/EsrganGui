namespace EsrganGui.Models
{
    /// <summary>主题选项，实际切换效果待后续实现。</summary>
    public enum AppTheme
    {
        Light = 0,
        Dark = 1,
        System = 2,
    }

    /// <summary>输出文件名的后缀生成方式。</summary>
    public enum SuffixMode
    {
        ModelName = 0,
        Scale = 1,
        Custom = 2,
    }

    /// <summary>保存在程序目录 preferences.json 中的首选项。</summary>
    public sealed class AppPreferences
    {
        public AppTheme Theme { get; set; } = AppTheme.Light;

        public SuffixMode SuffixMode { get; set; } = SuffixMode.ModelName;

        public string CustomSuffix { get; set; } = string.Empty;

        public AppPreferences Clone() => new()
        {
            Theme = Theme,
            SuffixMode = SuffixMode,
            CustomSuffix = CustomSuffix,
        };
    }
}
