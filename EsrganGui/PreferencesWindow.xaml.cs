using System.Windows;
using System.Windows.Controls;
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
            SuffixModeComboBox.SelectedIndex = (int)Preferences.SuffixMode;
            SuffixTextBox.Text = Preferences.CustomSuffix;
            UpdateSuffixTextBoxState();
        }

        private void SuffixModeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateSuffixTextBoxState();
        }

        private void UpdateSuffixTextBoxState()
        {
            if (SuffixTextBox is null || SuffixModeComboBox is null)
            {
                return;
            }

            SuffixTextBox.IsEnabled = SuffixModeComboBox.SelectedIndex == (int)SuffixMode.Custom;
        }

        private bool TrySave()
        {
            var preferences = new AppPreferences
            {
                Theme = (AppTheme)Math.Max(0, ThemeComboBox.SelectedIndex),
                SuffixMode = (SuffixMode)Math.Max(0, SuffixModeComboBox.SelectedIndex),
                CustomSuffix = SuffixTextBox.Text.Trim(),
            };

            if (preferences.SuffixMode == SuffixMode.Custom &&
                string.IsNullOrWhiteSpace(preferences.CustomSuffix))
            {
                MessageBox.Show(this, "请填写“后缀”内容，或改用其他后缀选项。", "首选项",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                SuffixTextBox.Focus();
                return false;
            }

            try
            {
                PreferencesStore.Save(preferences);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"无法保存首选项：{ex.Message}", "首选项",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            ThemeManager.Apply(preferences.Theme);

            Preferences = preferences;
            Saved = true;
            return true;
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
