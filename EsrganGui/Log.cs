using System.IO;
using System.Text;

namespace EsrganGui
{
    internal static class Log
    {
        public const string NcnnSource = "ncnn";
        public const string GuiSource = "esrgan_gui";

        private static readonly object SyncRoot = new();
        private static readonly UTF8Encoding Utf8NoBom = new(false);

        public static string FilePath => Path.Combine(AppContext.BaseDirectory, "log.txt");

        public static void Ncnn(string message) => Write(NcnnSource, message);

        public static void Gui(string message) => Write(GuiSource, message);

        public static void Write(string source, string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                return;
            }

            var timestamp = DateTime.Now.ToString("yyyyMMdd HH:mm:ss");
            var lines = message.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
            var builder = new StringBuilder();

            foreach (var line in lines)
            {
                if (line.Length == 0)
                {
                    continue;
                }

                builder.Append('(')
                    .Append(timestamp)
                    .Append(")[")
                    .Append(source)
                    .Append(']')
                    .AppendLine(line);
            }

            if (builder.Length == 0)
            {
                return;
            }

            lock (SyncRoot)
            {
                try
                {
                    File.AppendAllText(FilePath, builder.ToString(), Utf8NoBom);
                }
                catch
                {
                    // 日志写入失败不影响程序运行
                }
            }
        }

        public static bool Clear()
        {
            lock (SyncRoot)
            {
                try
                {
                    File.WriteAllText(FilePath, string.Empty, Utf8NoBom);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }
    }
}
