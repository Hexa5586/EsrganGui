using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace EsrganGui.Models
{
    /// <summary>
    /// 列表中的一条超分任务。
    /// </summary>
    public sealed class ImageJob : INotifyPropertyChanged
    {
        private int _index;
        private string _sourcePath = string.Empty;
        private string _outputPath = string.Empty;
        private bool _isCompleted;
        private bool _isFailed;
        private bool _showCompletedHighlight;

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
        public int Index
        {
            get => _index;
            set => SetField(ref _index, value);
        }

        /// <summary>原图路径。</summary>
        public string SourcePath
        {
            get => _sourcePath;
            set => SetField(ref _sourcePath, value);
        }

        /// <summary>输出图路径。</summary>
        public string OutputPath
        {
            get => _outputPath;
            set => SetField(ref _outputPath, value);
        }

        /// <summary>该项是否已成功完成放大。</summary>
        public bool IsCompleted
        {
            get => _isCompleted;
            set => SetField(ref _isCompleted, value);
        }

        public bool IsFailed
        {
            get => _isFailed;
            set => SetField(ref _isFailed, value);
        }

        public bool ShowCompletedHighlight
        {
            get => _showCompletedHighlight;
            set => SetField(ref _showCompletedHighlight, value);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return;
            }

            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
