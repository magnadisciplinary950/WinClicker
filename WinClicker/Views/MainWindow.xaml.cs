using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Linq;
using System.Threading.Tasks;
using WinClicker.Services;
using WinClicker.ViewModels;
using Windows.Graphics;
using WinUIEx;
using WinUIEx.Messaging;

namespace WinClicker.Views
{
    public sealed partial class MainWindow : WindowEx
    {
        public MainViewModel ViewModel { get; }

        private PointInt32 _lastPosition;

        private const int HotkeyId = 9000;

        private readonly nint _hwnd;

        private readonly bool _isPicking = false;
        private readonly CoordPopupService _coordPopupService;
        private readonly WindowMessageMonitor _messageMonitor;

        public MainWindow()
        {
            _coordPopupService = new CoordPopupService();

            ViewModel = new MainViewModel(
                new MouseService(),
                new OverlayPickerService(),
                _coordPopupService,
                new SettingsService()
            );

            InitializeComponent();

            Title = "WinClicker";
            Width = 480;
            MaxWidth = 480;
            Height = 384;
            MaxHeight = 384;

            IsMaximizable = false;
            IsResizable = false;
            IsAlwaysOnTop = true;

            AppWindow.TitleBar.PreferredTheme = TitleBarTheme.UseDefaultAppMode;
            AppWindow.SetIcon("Assets\\appicon.ico");

            ExtendsContentIntoTitleBar = true;
            SetTitleBar(AppTitleBar);

            _hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);

            Activated += MainWindow_Activated;
            Closed += MainWindow_Closed;

            _messageMonitor = new WindowMessageMonitor(this);
            _messageMonitor.WindowMessageReceived += OnWindowMessageReceived;

            RegisterHotkey();

            ViewModel.RequestHideWindow += () =>
            {
                _lastPosition = AppWindow.Position;

                AppWindow.Hide();
            };

            ViewModel.RequestShowWindow += () =>
            {
                AppWindow.Move(_lastPosition);
                AppWindow.Show();
                AppWindow.MoveInZOrderAtTop();
            };

            Closed += (sender, args) =>
            {
                Environment.Exit(0);
            };

            ViewModel.RequestRegisterHotkey += RegisterHotkey;
        }

        private void MainWindow_Activated(object sender, WindowActivatedEventArgs args)
        {
            if (!_isPicking && _coordPopupService != null && _coordPopupService.IsOpen)
            {
                _coordPopupService.IsOpen = false;
            }
        }

        private void MainWindow_Closed(object sender, WindowEventArgs args)
        {
            HotkeyService.UnregisterHotKey(_hwnd, HotkeyId);
        }
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = (TextBox)sender;

            if (string.IsNullOrEmpty(textBox.Text))
            {
                return;
            }

            if (textBox.Tag == null || !int.TryParse(textBox.Tag.ToString(), out int max))
            {
                max = 9999;
            }

            string digitsOnly = new([.. textBox.Text.Where(char.IsDigit)]);

            if (textBox.Text != digitsOnly)
            {
                textBox.Text = digitsOnly;
                textBox.SelectionStart = textBox.Text.Length;

                return;
            }

            if (int.TryParse(textBox.Text, out int value))
            {
                if (value > max)
                {
                    textBox.Text = max.ToString();
                    textBox.Select(textBox.Text.Length, 0);
                }
            }
        }

        private async void HotkeyInfoButton_Click(object sender, RoutedEventArgs e)
        {
            await Task.Delay(100);

            HotkeyTeachingTip.IsOpen = true;
        }

        public void RegisterHotkey()
        {
            HotkeyService.UnregisterHotKey(_hwnd, HotkeyId);

            var (mods, key) = HotkeyService.Parse(ViewModel.Hotkey);

            HotkeyService.RegisterHotKey(_hwnd, HotkeyId, mods, key);
        }

        private void OnWindowMessageReceived(object? sender, WindowMessageEventArgs e)
        {
            if ((long)e.Message.MessageId == 0x0312 && (long)e.Message.WParam == HotkeyId)
            {
                ViewModel.ToggleClickingCommand.Execute(null);

                e.Handled = true;
            }
        }
    }
}
