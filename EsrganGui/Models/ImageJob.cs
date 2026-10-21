namespace EsrganGui.Models
{
    /// <summary>
    /// 列表中的一条超分任务。
    /// </summary>
    public sealed class ImageJob
    {
        public ImageJob()
        {
        }

        public ImageJob(int index, string sourcePath, string outputPath)
        {
            Index = index;
            SourcePath = sourcePath;
            OutputPath = outputPath;
        }

        /// <summary>从 1 开始的十进制序号。</summary>
        public int Index { get; set; }

        /// <summary>原图路径。</summary>
        public string SourcePath { get; set; } = string.Empty;

        /// <summary>输出图路径。</summary>
        public string OutputPath { get; set; } = string.Empty;
    }
}
