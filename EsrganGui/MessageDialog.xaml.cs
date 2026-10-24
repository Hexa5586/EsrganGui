using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace EsrganGui
{
    public enum MessageDialogButtons
    {
        Confirm,
        ConfirmCancel,
        YesNoCancel,
    }

    public enum MessageDialogResult
    {
        Confirm,
        Yes,
        No,
        Cancel,
    }

    public partial class MessageDialog : Window
    {
        public MessageDialogResult Result { get; private set; } = MessageDialogResult.Cancel;

        private MessageDialog(string message, string caption, MessageDialogButtons buttons,
            MessageBoxImage icon, MessageDialogResult defaultResult)
        {
            InitializeComponent();
            ThemeManager.AttachTitleBar(this);

            Title = caption;
            MessageText.Text = message;
            ConfigureIcon(icon);
            ConfigureButtons(buttons, defaultResult);
            PreviewKeyDown += MessageDialog_PreviewKeyDown;
            Loaded += (_, _) => FocusDefaultButton();
        }

        public static MessageDialogResult Show(string messageBoxText, string caption,
            MessageDialogButtons buttons = MessageDialogButtons.Confirm,
            MessageBoxImage icon = MessageBoxImage.None,
            MessageDialogResult defaultResult = MessageDialogResult.Confirm)
        {
            var owner = Application.Current?.Windows
                .OfType<Window>()
                .FirstOrDefault(window => window.IsActive) ?? Application.Current?.MainWindow;

            return Show(owner, messageBoxText, caption, buttons, icon, defaultResult);
        }

        public static MessageDialogResult Show(Window? owner, string messageBoxText, string caption,
            MessageDialogButtons buttons = MessageDialogButtons.Confirm,
            MessageBoxImage icon = MessageBoxImage.None,
            MessageDialogResult defaultResult = MessageDialogResult.Confirm)
        {
            var dialog = new MessageDialog(messageBoxText, caption, buttons, icon, defaultResult);

            if (owner is not null && owner.IsLoaded)
            {
                dialog.Owner = owner;
                dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            }
            else
            {
                dialog.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }

            dialog.ShowDialog();
            return dialog.Result;
        }

        private void ConfigureIcon(MessageBoxImage icon)
        {
            string glyph;
            string brushKey;

            switch (icon)
            {
                case MessageBoxImage.Error:
                    glyph = "!";
                    brushKey = "MessageDialogErrorBrush";
                    break;
                case MessageBoxImage.Warning:
                    glyph = "!";
                    brushKey = "MessageDialogWarningBrush";
                    break;
                case MessageBoxImage.Question:
                    glyph = "?";
                    brushKey = "AccentBrush";
                    break;
                case MessageBoxImage.Information:
                    glyph = "i";
                    brushKey = "AccentBrush";
                    break;
                default:
                    IconPanel.Visibility = Visibility.Collapsed;
                    MessageScroll.Margin = new Thickness(0);
                    return;
            }

            IconGlyph.Text = glyph;
            IconGlyph.SetResourceReference(TextBlock.ForegroundProperty, brushKey);
        }

        private void ConfigureButtons(MessageDialogButtons buttons, MessageDialogResult defaultResult)
        {
            Button defaultButton;

            switch (buttons)
            {
                case MessageDialogButtons.Confirm:
                    defaultButton = ConfirmButton;
                    break;
                case MessageDialogButtons.ConfirmCancel:
                    CancelButton.Visibility = Visibility.Visible;
                    defaultButton = defaultResult == MessageDialogResult.Cancel
                        ? CancelButton
                        : ConfirmButton;
                    break;
                case MessageDialogButtons.YesNoCancel:
                    ConfirmButton.Visibility = Visibility.Collapsed;
                    YesButton.Visibility = Visibility.Visible;
                    NoButton.Visibility = Visibility.Visible;
                    CancelButton.Visibility = Visibility.Visible;
                    defaultButton = defaultResult switch
                    {
                        MessageDialogResult.Yes => YesButton,
                        MessageDialogResult.Cancel => CancelButton,
                        _ => NoButton,
                    };
                    break;
                default:
                    defaultButton = ConfirmButton;
                    break;
            }

            ApplyDefaultStyle(defaultButton);
            defaultButton.IsDefault = true;
        }

        private void ApplyDefaultStyle(Button defaultButton)
        {
            var accentStyle = (Style)FindResource("AccentButtonStyle");
            var normalStyle = (Style)FindResource("ButtonStyle");

            ConfirmButton.Style = ReferenceEquals(defaultButton, ConfirmButton) ? accentStyle : normalStyle;
            YesButton.Style = ReferenceEquals(defaultButton, YesButton) ? accentStyle : normalStyle;
            NoButton.Style = ReferenceEquals(defaultButton, NoButton) ? accentStyle : normalStyle;
            CancelButton.Style = ReferenceEquals(defaultButton, CancelButton) ? accentStyle : normalStyle;
        }

        private void FocusDefaultButton()
        {
            if (ConfirmButton.Visibility == Visibility.Visible && ConfirmButton.IsDefault)
            {
                ConfirmButton.Focus();
            }
            else if (YesButton.Visibility == Visibility.Visible && YesButton.IsDefault)
            {
                YesButton.Focus();
            }
            else if (NoButton.Visibility == Visibility.Visible && NoButton.IsDefault)
            {
                NoButton.Focus();
            }
            else if (CancelButton.Visibility == Visibility.Visible)
            {
                CancelButton.Focus();
            }
        }

        private void MessageDialog_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Escape)
            {
                return;
            }

            Result = MessageDialogResult.Cancel;
            DialogResult = true;
            e.Handled = true;
        }

        private void Complete(MessageDialogResult result)
        {
            Result = result;
            DialogResult = true;
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e) => Complete(MessageDialogResult.Confirm);

        private void YesButton_Click(object sender, RoutedEventArgs e) => Complete(MessageDialogResult.Yes);

        private void NoButton_Click(object sender, RoutedEventArgs e) => Complete(MessageDialogResult.No);

        private void CancelButton_Click(object sender, RoutedEventArgs e) => Complete(MessageDialogResult.Cancel);
    }
}
