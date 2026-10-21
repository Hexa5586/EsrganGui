using System.IO;

namespace EsrganGui
{
    /// <summary>
    /// 与可执行文件同级的工具路径。Windows 与 Linux 构建通过编译符号切换可执行文件名。
    /// </summary>
    internal static class ToolPaths
    {
#if ESRGAN_LINUX
        public const string EngineFileName = "realesrgan-ncnn-vulkan";
        public const string FfmpegFileName = "ffmpeg";
#else
        public const string EngineFileName = "realesrgan-ncnn-vulkan.exe";
        public const string FfmpegFileName = "ffmpeg.exe";
#endif

        /// <summary>realesrgan-ncnn-vulkan 引擎路径。</summary>
        public static string EnginePath =>
            Path.Combine(AppContext.BaseDirectory, "realesrgan", EngineFileName);

        /// <summary>模型目录（realesrgan\models）。</summary>
        public static string ModelsDirectory =>
            Path.Combine(AppContext.BaseDirectory, "realesrgan", "models");

        /// <summary>ffmpeg 路径。</summary>
        public static string FfmpegPath =>
            Path.Combine(AppContext.BaseDirectory, "ffmpeg", FfmpegFileName);
    }
}
