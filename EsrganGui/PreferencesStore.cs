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
                var preferences = JsonSerializer.Deserialize<AppPreferences>(json, JsonOptions) ?? new AppPreferences();
                preferences.SuffixTemplate ??= "%MODEL%";
                MigrateLegacySuffix(json, preferences);
                return preferences;
            }
            catch
            {
                // 配置损坏或无法读取时回退到默认值
                return new AppPreferences();
            }
        }

        /// <summary>把旧版 suffixMode / customSuffix 迁移到新的后缀模板。</summary>
        private static void MigrateLegacySuffix(string json, AppPreferences preferences)
        {
            try
            {
                using var document = JsonDocument.Parse(json);
                var root = document.RootElement;
                if (root.ValueKind != JsonValueKind.Object ||
                    root.TryGetProperty("suffixTemplate", out _) ||
                    !root.TryGetProperty("suffixMode", out var modeElement))
                {
                    return;
                }

                var mode = modeElement.GetString();
                if (string.Equals(mode, "Scale", StringComparison.OrdinalIgnoreCase))
                {
                    preferences.SuffixTemplate = "%SCALE%";
                }
                else if (string.Equals(mode, "Custom", StringComparison.OrdinalIgnoreCase))
                {
                    var custom = root.TryGetProperty("customSuffix", out var customElement)
                        ? customElement.GetString()
                        : null;
                    preferences.SuffixTemplate = string.IsNullOrWhiteSpace(custom) ? "%MODEL%" : custom;
                }
                else
                {
                    preferences.SuffixTemplate = "%MODEL%";
                }
            }
            catch
            {
                // 迁移失败时保留新字段的默认值
            }
        }

        public static void Save(AppPreferences preferences)
        {
            var json = JsonSerializer.Serialize(preferences, JsonOptions);
            File.WriteAllText(FilePath, json);
        }
    }
}
