using System.Diagnostics;
using System.IO;
using System.Windows;
using EsrganGui.Models;

namespace EsrganGui
{
    /// <summary>首选项窗口：读取 / 保存与可执行文件同级的 preferences.json。</summary>
    public partial class PreferencesWindow : Window
    {
        /// <summary>最近一次成功保存的首选项。</summary>
        public AppPreferences Preferences { get; private set; }

        /// <summary>本次打开窗口期间是否至少成功保存过一次（应用或确定）。</summary>
        public bool Saved { get; private set; }

        public PreferencesWindow(AppPreferences preferences)
        {
            InitializeComponent();
            ThemeManager.AttachTitleBar(this);

            Preferences = preferences.Clone();
            ThemeComboBox.SelectedIndex = (int)Preferences.Theme;
            SuffixTextBox.Text = Preferences.SuffixTemplate;
            RemoveTrailingUnderscoreCheckBox.IsChecked = Preferences.RemoveTrailingUnderscore;
            DeleteUncompressedCheckBox.IsChecked = Preferences.DeleteUncompressedOutput;
        }

        private bool TrySave()
        {
            var preferences = new AppPreferences
            {
                Theme = (AppTheme)Math.Max(0, ThemeComboBox.SelectedIndex),
                SuffixTemplate = SuffixTextBox.Text.Trim(),
                RemoveTrailingUnderscore = RemoveTrailingUnderscoreCheckBox.IsChecked == true,
                DeleteUncompressedOutput = DeleteUncompressedCheckBox.IsChecked == true,
            };

            var themeChanged = preferences.Theme != Preferences.Theme;

            try
            {
                PreferencesStore.Save(preferences);
            }
            catch (Exception ex)
            {
                MessageDialog.Show(this, $"无法保存首选项：{ex.Message}", "首选项",
                    MessageDialogButtons.Confirm, MessageBoxImage.Warning);
                return false;
            }

            // 仅在主题字段确实变化时才应用主题，避免全局重绘/闪烁
            if (themeChanged)
            {
                ThemeManager.Apply(preferences.Theme);
            }

            Preferences = preferences;
            Saved = true;
            return true;
        }

        private void ClearLogButton_Click(object sender, RoutedEventArgs e)
        {
            if (!Log.Clear())
            {
                MessageDialog.Show(this, "无法清空日志文件，请确认文件未被其他程序占用。", "日志",
                    MessageDialogButtons.Confirm, MessageBoxImage.Warning);
            }
        }

        private void OpenLogButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!File.Exists(Log.FilePath))
                {
                    MessageDialog.Show(this, "日志文件尚不存在。", "日志",
                        MessageDialogButtons.Confirm, MessageBoxImage.Information);
                    return;
                }

                Process.Start(new ProcessStartInfo(Log.FilePath)
                {
                    UseShellExecute = true,
                });
            }
            catch (Exception ex)
            {
                MessageDialog.Show(this, $"无法打开日志文件：{ex.Message}", "日志",
                    MessageDialogButtons.Confirm, MessageBoxImage.Warning);
            }
        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            TrySave();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (TrySave())
            {
                DialogResult = true;
            }
        }
    }
}
