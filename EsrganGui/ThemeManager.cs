using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;
using EsrganGui.Models;
using Microsoft.Win32;

namespace EsrganGui
{
    /// <summary>切换应用主题（Light / Dark / 跟随系统）。</summary>
    internal static class ThemeManager
    {
        private const int DwmUseImmersiveDarkModeBefore20H1 = 19;
        private const int DwmUseImmersiveDarkMode = 20;

        private const uint SwpNoSize = 0x0001;
        private const uint SwpNoMove = 0x0002;
        private const uint SwpNoZOrder = 0x0004;
        private const uint SwpFrameChanged = 0x0020;

        private const uint RdwInvalidate = 0x0001;
        private const uint RdwAllChildren = 0x0080;
        private const uint RdwUpdateNow = 0x0100;
        private const uint RdwFrame = 0x0400;

        /// <summary>当前是否使用暗色主题。</summary>
        public static bool IsDark { get; private set; }

        public static void Apply(AppTheme theme)
        {
            var application = Application.Current;
            if (application is null)
            {
                return;
            }

            var useDark = theme switch
            {
                AppTheme.Dark => true,
                AppTheme.System => IsSystemDarkTheme(),
                _ => false,
            };

            var themeSource = useDark ? "Themes/Dark.xaml" : "Themes/Light.xaml";

            ResourceDictionary? existing = null;
            foreach (var dictionary in application.Resources.MergedDictionaries)
            {
                var path = dictionary.Source?.OriginalString;
                if (path is not null &&
                    (path.EndsWith("Themes/Light.xaml", StringComparison.OrdinalIgnoreCase) ||
                     path.EndsWith("Themes/Dark.xaml", StringComparison.OrdinalIgnoreCase)))
                {
                    existing = dictionary;
                    break;
                }
            }

            var alreadyApplied = existing?.Source?.OriginalString
                .EndsWith(themeSource, StringComparison.OrdinalIgnoreCase) == true;

            if (!alreadyApplied)
            {
                var dictionary = new ResourceDictionary
                {
                    Source = new Uri(themeSource, UriKind.Relative),
                };

                if (existing is not null)
                {
                    application.Resources.MergedDictionaries.Remove(existing);
                }

                application.Resources.MergedDictionaries.Add(dictionary);
            }

            IsDark = useDark;
            UpdateTitleBars();

            // 已打开的窗口在切换主题时可能来不及重绘标题栏，空闲时再补一次
            application.Dispatcher.BeginInvoke(new Action(UpdateTitleBars), DispatcherPriority.ApplicationIdle);
        }

        /// <summary>让窗口标题栏跟随主题（Windows 10 1809+ / Windows 11）。</summary>
        public static void AttachTitleBar(Window window)
        {
            window.SourceInitialized += (_, _) => ApplyTitleBar(window);

            if (window.IsLoaded)
            {
                ApplyTitleBar(window);
            }
        }

        private static void UpdateTitleBars()
        {
            var application = Application.Current;
            if (application is null)
            {
                return;
            }

            foreach (Window window in application.Windows)
            {
                ApplyTitleBar(window);
            }
        }

        private static void ApplyTitleBar(Window window)
        {
            var handle = new WindowInteropHelper(window).Handle;
            if (handle == IntPtr.Zero)
            {
                return;
            }

            var value = IsDark ? 1 : 0;
            if (DwmSetWindowAttribute(handle, DwmUseImmersiveDarkMode, ref value, sizeof(int)) != 0)
            {
                DwmSetWindowAttribute(handle, DwmUseImmersiveDarkModeBefore20H1, ref value, sizeof(int));
            }

            // 触发非客户区重绘，确保标题栏立即换色
            SetWindowPos(handle, IntPtr.Zero, 0, 0, 0, 0,
                SwpNoSize | SwpNoMove | SwpNoZOrder | SwpFrameChanged);
            RedrawWindow(handle, IntPtr.Zero, IntPtr.Zero,
                RdwFrame | RdwInvalidate | RdwUpdateNow | RdwAllChildren);
            DwmFlush();
        }

        private static bool IsSystemDarkTheme()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(
                    @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
                return key?.GetValue("AppsUseLightTheme") is int value && value == 0;
            }
            catch
            {
                return false;
            }
        }

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attribute, ref int value, int size);

        [DllImport("dwmapi.dll")]
        private static extern int DwmFlush();

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter,
            int x, int y, int cx, int cy, uint flags);

        [DllImport("user32.dll")]
        private static extern bool RedrawWindow(IntPtr hWnd, IntPtr updateRect, IntPtr updateRegion, uint flags);
    }
}
