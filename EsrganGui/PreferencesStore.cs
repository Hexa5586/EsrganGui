using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using EsrganGui.Models;

namespace EsrganGui
{
    /// <summary>读取 / 保存与可执行文件同级的 preferences.json。</summary>
    internal static class PreferencesStore
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter() },
        };

        public static string FilePath => Path.Combine(AppContext.BaseDirectory, "preferences.json");

        public static AppPreferences Load()
        {
            try
            {
                if (!File.Exists(FilePath))
                {
                    return new AppPreferences();
                }

                var json = File.ReadAllText(FilePath);
                return JsonSerializer.Deserialize<AppPreferences>(json, JsonOptions) ?? new AppPreferences();
            }
            catch
            {
                // 配置损坏或无法读取时回退到默认值
                return new AppPreferences();
            }
        }

        public static void Save(AppPreferences preferences)
        {
            var json = JsonSerializer.Serialize(preferences, JsonOptions);
            File.WriteAllText(FilePath, json);
        }
    }
}
