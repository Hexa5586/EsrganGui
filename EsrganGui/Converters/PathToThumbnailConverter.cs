using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace EsrganGui.Converters
{
    /// <summary>
    /// 把图片路径转换成低分辨率缩略图，用于悬浮预览。
    /// 路径无效或图片无法解码时返回 null。
    /// </summary>
    public sealed class PathToThumbnailConverter : IValueConverter
    {
        /// <summary>缩略图的最大解码宽度，越小预览生成得越快。</summary>
        public int DecodePixelWidth { get; set; } = 320;

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not string path || string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                return null;
            }

            try
            {
                var thumbnail = new BitmapImage();
                thumbnail.BeginInit();
                thumbnail.CacheOption = BitmapCacheOption.OnLoad;
                thumbnail.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                thumbnail.DecodePixelWidth = DecodePixelWidth;
                thumbnail.UriSource = new Uri(path, UriKind.Absolute);
                thumbnail.EndInit();
                thumbnail.Freeze();
                return thumbnail;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
