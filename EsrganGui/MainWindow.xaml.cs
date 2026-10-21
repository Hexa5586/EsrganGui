using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using EsrganGui.Models;
using Microsoft.Win32;

namespace EsrganGui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const double IndexColumnWidth = 72;
        private const double MinPathColumnWidth = 140;

        private static readonly HashSet<string> SupportedImageExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".png", ".jpg", ".jpeg", ".bmp", ".webp", ".tif", ".tiff",
        };

        private string? _sortProperty;
        private ListSortDirection _sortDirection = ListSortDirection.Ascending;
        private GridViewColumnHeader? _pressedHeader;
        private Point? _headerPressPosition;

        private AppPreferences _preferences = new();

        private Point? _marqueeOrigin;
        private Point? _marqueeLastPoint;
        private bool _isMarqueeActive;
        private bool _marqueeAdditive;
        private readonly HashSet<object> _marqueeBaseSelection = [];
        private ScrollViewer? _listScrollViewer;

        /// <summary>
        /// 待处理的任务列表，可通过“添加图片...”按钮或从资源管理器拖拽加入。
        /// </summary>
        public ObservableCollection<ImageJob> Jobs { get; } = [];

        /// <summary>从程序目录下的 realesrgan\models 读取到的模型名称（不含扩展名）。</summary>
        public ObservableCollection<string> Models { get; } = [];

        public MainWindow()
        {
            InitializeComponent();
            _preferences = PreferencesStore.Load();
            DataContext = this;
            LoadModels();
            FitToWorkArea();
            Jobs.CollectionChanged += (_, _) => UpdateColumnWidths();
        }

        /// <summary>扫描 realesrgan\models，加载具有 .param + .bin 配对的模型。</summary>
        private void LoadModels()
        {
            Models.Clear();

            var modelsDirectory = ToolPaths.ModelsDirectory;
            if (!Directory.Exists(modelsDirectory))
            {
                return;
            }

            var modelNames = new List<string>();
            foreach (var paramFile in Directory.EnumerateFiles(modelsDirectory, "*.param"))
            {
                var name = Path.GetFileNameWithoutExtension(paramFile);
                if (string.IsNullOrEmpty(name) ||
                    !File.Exists(Path.Combine(modelsDirectory, name + ".bin")))
                {
                    continue;
                }

                modelNames.Add(name);
            }

            modelNames.Sort(StringComparer.OrdinalIgnoreCase);
            foreach (var name in modelNames)
            {
                Models.Add(name);
            }

            ModelComboBox.SelectedIndex = Models.Count > 0 ? 0 : -1;
        }

        /// <summary>高 DPI 屏幕下窗口可能超出可用区域，这里按屏幕工作区收缩。</summary>
        private void FitToWorkArea()
        {
            var workArea = SystemParameters.WorkArea;
            if (!workArea.IsEmpty)
            {
                Width = Math.Min(Width, Math.Max(MinWidth, workArea.Width * 0.9));
                Height = Math.Min(Height, Math.Max(MinHeight, workArea.Height * 0.9));
            }
        }

        /// <summary>列表列宽随窗口宽度自动分配，避免出现横向滚动条。</summary>
        private void JobList_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateColumnWidths();
        }

        private void UpdateColumnWidths()
        {
            if (JobList.View is not GridView gridView || JobList.ActualWidth <= 0)
            {
                return;
            }

            // 列可能被拖动重排，按标识而不是索引查找
            GridViewColumn? indexColumn = null;
            var pathColumns = new List<GridViewColumn>(2);
            foreach (var column in gridView.Columns)
            {
                if (column.Header is not GridViewColumnHeader { Tag: string tag })
                {
                    continue;
                }

                if (tag == "Index")
                {
                    indexColumn = column;
                }
                else if (tag is "SourcePath" or "OutputPath")
                {
                    pathColumns.Add(column);
                }
            }

            if (indexColumn is null || pathColumns.Count == 0)
            {
                return;
            }

            // 行高 38、表头高 36，出现纵向滚动条时扣除其宽度
            var needsVerticalScrollBar = Jobs.Count * 38 + 36 > JobList.ActualHeight;
            var scrollBarWidth = needsVerticalScrollBar ? SystemParameters.VerticalScrollBarWidth : 0;

            var remaining = JobList.ActualWidth - IndexColumnWidth - scrollBarWidth;
            if (remaining <= 0)
            {
                return;
            }

            var pathColumnWidth = Math.Max(MinPathColumnWidth, remaining / pathColumns.Count);
            indexColumn.Width = IndexColumnWidth;
            foreach (var column in pathColumns)
            {
                column.Width = pathColumnWidth;
            }
        }

        /// <summary>
        /// 列标题没有触发 ButtonBase.Click，这里用 Preview 鼠标事件自行判定“点击”，
        /// 并排除拖动列宽 / 拖动调整列顺序的情况。
        /// </summary>
        private void JobList_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var source = e.OriginalSource as DependencyObject;

            _pressedHeader = FindAncestor<GridViewColumnHeader>(source);
            _headerPressPosition = _pressedHeader is null ? null : e.GetPosition(JobList);

            // 在表头或滚动条上按下时不启动框选
            if (_pressedHeader is null && FindAncestor<ScrollBar>(source) is null)
            {
                _marqueeOrigin = e.GetPosition(JobList);
                _marqueeLastPoint = null;
            }
            else
            {
                _marqueeOrigin = null;
            }
        }

        private void JobList_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var header = _pressedHeader;
            var pressPosition = _headerPressPosition;
            _pressedHeader = null;
            _headerPressPosition = null;

            // 拖动框选结束时吞掉这次抬起，避免误触路径单元格的“打开所在位置”
            if (_isMarqueeActive)
            {
                EndMarquee();
                e.Handled = true;
                return;
            }

            _marqueeOrigin = null;
            _marqueeLastPoint = null;

            if (header is null || pressPosition is null || header.Role == GridViewColumnHeaderRole.Padding)
            {
                return;
            }

            // 拖动超过阈值视为调整列宽或列顺序，不触发排序
            var releasePosition = e.GetPosition(JobList);
            if (Math.Abs(releasePosition.X - pressPosition.Value.X) > 4 ||
                Math.Abs(releasePosition.Y - pressPosition.Value.Y) > 4)
            {
                return;
            }

            if (!ReferenceEquals(FindAncestor<GridViewColumnHeader>(e.OriginalSource as DependencyObject), header))
            {
                return;
            }

            SortByHeader(header);
        }

        /// <summary>按住鼠标左键拖出矩形，选中与矩形相交的列表项；按住 Ctrl 时追加到已有选区。</summary>
        private void JobList_MouseMove(object sender, MouseEventArgs e)
        {
            if (_marqueeOrigin is not { } origin || e.LeftButton != MouseButtonState.Pressed)
            {
                return;
            }

            var current = e.GetPosition(JobList);
            if (!_isMarqueeActive)
            {
                if (Math.Abs(current.X - origin.X) < 4 && Math.Abs(current.Y - origin.Y) < 4)
                {
                    return;
                }

                _isMarqueeActive = true;
                _marqueeAdditive = (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control;

                _marqueeBaseSelection.Clear();
                if (_marqueeAdditive)
                {
                    foreach (var item in JobList.SelectedItems)
                    {
                        _marqueeBaseSelection.Add(item);
                    }
                }

                JobList.CaptureMouse();
            }

            if (_marqueeLastPoint is { } last &&
                Math.Abs(last.X - current.X) < 0.5 &&
                Math.Abs(last.Y - current.Y) < 0.5)
            {
                return;
            }

            _marqueeLastPoint = current;

            var marquee = new Rect(origin, current);
            var contentArea = GetListContentArea();
            if (!contentArea.IsEmpty)
            {
                marquee.Intersect(contentArea);
            }

            UpdateMarqueeVisual(marquee);
            UpdateMarqueeSelection(marquee);
            e.Handled = true;
        }

        private void JobList_LostMouseCapture(object sender, MouseEventArgs e)
        {
            if (_isMarqueeActive)
            {
                EndMarquee();
            }
        }

        private void EndMarquee()
        {
            _isMarqueeActive = false;
            _marqueeOrigin = null;
            _marqueeLastPoint = null;
            _marqueeBaseSelection.Clear();
            MarqueeRectangle.Visibility = Visibility.Collapsed;

            if (JobList.IsMouseCaptured)
            {
                JobList.ReleaseMouseCapture();
            }
        }

        private void UpdateMarqueeVisual(Rect marquee)
        {
            if (marquee.IsEmpty || marquee.Width < 1 || marquee.Height < 1)
            {
                MarqueeRectangle.Visibility = Visibility.Collapsed;
                return;
            }

            MarqueeRectangle.Visibility = Visibility.Visible;
            MarqueeRectangle.Width = marquee.Width;
            MarqueeRectangle.Height = marquee.Height;
            Canvas.SetLeft(MarqueeRectangle, marquee.X);
            Canvas.SetTop(MarqueeRectangle, marquee.Y);
        }

        private void UpdateMarqueeSelection(Rect marquee)
        {
            var hitItems = new List<object>();
            if (!marquee.IsEmpty)
            {
                foreach (var item in JobList.Items)
                {
                    if (JobList.ItemContainerGenerator.ContainerFromItem(item) is not ListViewItem container)
                    {
                        continue;
                    }

                    // 只处理已生成容器的可见行
                    var bounds = container.TransformToAncestor(JobList)
                        .TransformBounds(new Rect(container.RenderSize));
                    if (marquee.IntersectsWith(bounds))
                    {
                        hitItems.Add(item);
                    }
                }
            }

            var desired = new HashSet<object>(hitItems);
            if (_marqueeAdditive)
            {
                desired.UnionWith(_marqueeBaseSelection);
            }

            for (var i = JobList.SelectedItems.Count - 1; i >= 0; i--)
            {
                var selectedItem = JobList.SelectedItems[i];
                if (selectedItem is null || !desired.Contains(selectedItem))
                {
                    JobList.SelectedItems.RemoveAt(i);
                }
            }

            foreach (var item in desired)
            {
                if (!JobList.SelectedItems.Contains(item))
                {
                    JobList.SelectedItems.Add(item);
                }
            }
        }

        private Rect GetListContentArea()
        {
            _listScrollViewer ??= FindVisualChild<ScrollViewer>(JobList);
            if (_listScrollViewer is null)
            {
                return new Rect(0, 0, JobList.ActualWidth, JobList.ActualHeight);
            }

            var topLeft = _listScrollViewer.TransformToAncestor(JobList).Transform(new Point(0, 0));
            return new Rect(topLeft.X, topLeft.Y, _listScrollViewer.ActualWidth, _listScrollViewer.ActualHeight);
        }

        private static T? FindVisualChild<T>(DependencyObject current) where T : DependencyObject
        {
            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(current); i++)
            {
                var child = VisualTreeHelper.GetChild(current, i);
                if (child is T match)
                {
                    return match;
                }

                if (FindVisualChild<T>(child) is { } descendant)
                {
                    return descendant;
                }
            }

            return null;
        }

        private static T? FindAncestor<T>(DependencyObject? current) where T : DependencyObject
        {
            while (current is not null and not T)
            {
                current = current is Visual or Visual3D
                    ? VisualTreeHelper.GetParent(current)
                    : LogicalTreeHelper.GetParent(current);
            }

            return current as T;
        }

        /// <summary>点击列标题按该列排序，再次点击切换升序/降序。</summary>
        private void SortByHeader(GridViewColumnHeader header)
        {
            if (header.Tag is not string property)
            {
                return;
            }

            var direction = string.Equals(_sortProperty, property, StringComparison.Ordinal) &&
                            _sortDirection == ListSortDirection.Ascending
                ? ListSortDirection.Descending
                : ListSortDirection.Ascending;

            _sortProperty = property;
            _sortDirection = direction;

            if (CollectionViewSource.GetDefaultView(JobList.ItemsSource) is ListCollectionView view)
            {
                using (view.DeferRefresh())
                {
                    view.SortDescriptions.Clear();
                    view.SortDescriptions.Add(new SortDescription(property, direction));
                }
            }

            UpdateSortIndicator(header, direction);
        }

        private void UpdateSortIndicator(GridViewColumnHeader activeHeader, ListSortDirection direction)
        {
            if (JobList.View is not GridView gridView)
            {
                return;
            }

            foreach (var column in gridView.Columns)
            {
                if (column.Header is not GridViewColumnHeader header || header.Content is not string title)
                {
                    continue;
                }

                var baseTitle = title.TrimEnd(' ', '▲', '▼');
                header.Content = ReferenceEquals(header, activeHeader)
                    ? baseTitle + (direction == ListSortDirection.Ascending ? " ▲" : " ▼")
                    : baseTitle;
            }
        }

        /// <summary>点击路径单元格：打开文件所在位置并选中该文件。</summary>
        private void PathCell_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is TextBlock { Text: var path } && !string.IsNullOrWhiteSpace(path))
            {
                OpenInExplorer(path);
            }
        }

        private static void OpenInExplorer(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    Process.Start(new ProcessStartInfo("explorer.exe", $"/select,\"{path}\"")
                    {
                        UseShellExecute = true,
                    });
                }
                else if (Directory.Exists(path))
                {
                    Process.Start(new ProcessStartInfo("explorer.exe", $"\"{path}\"")
                    {
                        UseShellExecute = true,
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"无法打开所在位置：{ex.Message}", "ESRGAN GUI",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void QualityTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // 只允许输入十进制数字
            e.Handled = !e.Text.All(char.IsDigit);
        }

        private void QualityTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(QualityTextBox.Text.Trim(), out var quality))
            {
                quality = 90;
            }

            QualityTextBox.Text = Math.Clamp(quality, 1, 100).ToString();
        }

        /// <summary>添加或修改输出选项后，立即刷新所有任务的输出路径。</summary>
        private void OutputSettings_Changed(object sender, RoutedEventArgs e)
        {
            RecalculateOutputPaths();
        }

        private void ModelComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RecalculateOutputPaths();
        }

        private void OutputModeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (OutputDirectoryLabel is null || OutputDirectoryTextBox is null)
            {
                return;
            }

            var useSpecifiedDirectory = OutputModeComboBox.SelectedIndex == 1;
            OutputDirectoryLabel.Text = useSpecifiedDirectory ? "输出目录" : "追加子目录";
            OutputDirectoryTextBox.ToolTip = useSpecifiedDirectory
                ? "输出图片的保存目录，所有输出会放在此目录下"
                : "在原图所在目录下追加的子目录，可留空";

            RecalculateOutputPaths();
        }

        private void OutputDirectoryTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            RecalculateOutputPaths();
        }

        private void RecalculateOutputPaths()
        {
            foreach (var job in Jobs)
            {
                job.OutputPath = BuildOutputPath(job.SourcePath);
            }
        }

        /// <summary>根据当前模型、压缩设置和输出选项生成输出路径。</summary>
        private string BuildOutputPath(string sourcePath)
        {
            var baseName = Path.GetFileNameWithoutExtension(sourcePath);
            var extension = CompressCheckBox.IsChecked == true ? ".jpg" : ".png";
            var fileName = $"{baseName}_{GetOutputSuffix()}{extension}";

            var outputOption = OutputDirectoryTextBox is null ? string.Empty : OutputDirectoryTextBox.Text.Trim();

            if (OutputModeComboBox is not null && OutputModeComboBox.SelectedIndex == 1)
            {
                // 指定输出目录为空时退回原图目录，避免生成相对路径
                var targetDirectory = string.IsNullOrEmpty(outputOption)
                    ? Path.GetDirectoryName(sourcePath) ?? string.Empty
                    : outputOption;
                return Path.Combine(targetDirectory, fileName);
            }

            var sourceDirectory = Path.GetDirectoryName(sourcePath) ?? string.Empty;
            return Path.Combine(sourceDirectory, outputOption, fileName);
        }

        /// <summary>按首选项决定输出文件的后缀。</summary>
        private string GetOutputSuffix()
        {
            var modelName = ModelComboBox.SelectedItem as string;

            return _preferences.SuffixMode switch
            {
                SuffixMode.Scale => GetScaleSuffix(modelName),
                SuffixMode.Custom => string.IsNullOrWhiteSpace(_preferences.CustomSuffix)
                    ? "out"
                    : _preferences.CustomSuffix.Trim(),
                _ => string.IsNullOrWhiteSpace(modelName) ? "out" : modelName,
            };
        }

        /// <summary>从模型名中解析放大倍数（如 realesr-animevideov3-x2 → x2）。</summary>
        private static string GetScaleSuffix(string? modelName)
        {
            if (string.IsNullOrWhiteSpace(modelName))
            {
                return "out";
            }

            var match = Regex.Match(modelName, @"x(\d+)", RegexOptions.IgnoreCase);
            return match.Success ? "x" + match.Groups[1].Value : "out";
        }

        /// <summary>打开首选项窗口；保存后立即刷新输出路径。</summary>
        private void PreferencesButton_Click(object sender, RoutedEventArgs e)
        {
            var window = new PreferencesWindow(_preferences)
            {
                Owner = this,
            };

            window.ShowDialog();

            if (window.Saved)
            {
                _preferences = window.Preferences;
                RecalculateOutputPaths();
            }
        }

        /// <summary>选择本地图片并追加到任务列表。</summary>
        private void AddImagesButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Title = "选择要处理的图片",
                Filter = "图片文件|*.png;*.jpg;*.jpeg;*.bmp;*.webp;*.tif;*.tiff|所有文件|*.*",
                Multiselect = true,
                CheckFileExists = true,
            };

            if (dialog.ShowDialog(this) != true)
            {
                return;
            }

            AddImageFiles(dialog.FileNames);
        }

        /// <summary>从资源管理器拖入文件时接受文件拖放。</summary>
        private void JobList_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop)
                ? DragDropEffects.Copy
                : DragDropEffects.None;
            e.Handled = true;
        }

        private void JobList_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetData(DataFormats.FileDrop) is not string[] paths)
            {
                return;
            }

            AddImageFiles(paths);
            e.Handled = true;
        }

        private void AddImageFiles(IEnumerable<string> paths)
        {
            foreach (var path in ExpandPaths(paths))
            {
                if (!SupportedImageExtensions.Contains(Path.GetExtension(path)))
                {
                    continue;
                }

                if (Jobs.Any(job => string.Equals(job.SourcePath, path, StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                Jobs.Add(new ImageJob(Jobs.Count + 1, path, BuildOutputPath(path)));
            }
        }

        /// <summary>拖入文件夹时展开其中的一级文件。</summary>
        private static IEnumerable<string> ExpandPaths(IEnumerable<string> paths)
        {
            foreach (var path in paths)
            {
                if (Directory.Exists(path))
                {
                    foreach (var file in Directory.EnumerateFiles(path))
                    {
                        yield return file;
                    }
                }
                else if (File.Exists(path))
                {
                    yield return path;
                }
            }
        }

        /// <summary>从列表中删除选中的任务，并重新排列序号。</summary>
        private void RemoveSelectedButton_Click(object sender, RoutedEventArgs e)
        {
            var selected = JobList.SelectedItems.Cast<ImageJob>().ToList();
            if (selected.Count == 0)
            {
                return;
            }

            // TODO（待执行功能完成后实现）：若选中的任务“放大完成”，删除前应询问是否一并删除放大后的图片，
            // 默认选择“否”；无论如何都不删除原始图片。目前还没有放大完成状态，所以这里只从列表移除。
            foreach (var job in selected)
            {
                Jobs.Remove(job);
            }

            RenumberJobs();
        }

        private void RenumberJobs()
        {
            for (var i = 0; i < Jobs.Count; i++)
            {
                Jobs[i].Index = i + 1;
            }
        }
    }
}
