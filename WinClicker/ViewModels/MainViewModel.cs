using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Threading;
using System.Threading.Tasks;
using WinClicker.Services;

namespace WinClicker.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        public event Action? RequestHideWindow;
        public event Action? RequestShowWindow;
        public event Action? RequestRegisterHotkey;

        private readonly IOverlayPickerService _pickerService;
        private readonly CoordPopupService _coordPopupService;
        private readonly IMouseService _mouseService;
        private readonly SettingsService _settings;

        private CancellationTokenSource? _cts;

        public MainViewModel(IMouseService mouseService, IOverlayPickerService pickerService, CoordPopupService coordPopupService, SettingsService settingsService)
        {
            _mouseService = mouseService;
            _pickerService = pickerService;
            _coordPopupService = coordPopupService;

            _settings = settingsService;

            _settings.Load(this);
        }

        [ObservableProperty] public partial string Hours { get; set; } = "0";
        [ObservableProperty] public partial string Minutes { get; set; } = "0";
        [ObservableProperty] public partial string Seconds { get; set; } = "0";
        [ObservableProperty] public partial string Milliseconds { get; set; } = "100";

        [ObservableProperty] public partial string XCoord { get; set; } = "0";
        [ObservableProperty] public partial string YCoord { get; set; } = "0";

        [ObservableProperty] public partial int CoordModeIndex { get; set; } = 0;

        [ObservableProperty] public partial int MouseButtonIndex { get; set; } = 0; // 0: Left, 1: Right, 2: Middle
        [ObservableProperty] public partial int ClickTypeIndex { get; set; } = 0; // 0: Single, 1: Double

        [ObservableProperty] public partial string Hotkey { get; set; } = "Ctrl+Shift+C";

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsNotClicking))]
        public partial bool IsClicking { get; set; }

        public bool IsNotClicking => !IsClicking;

        protected override void OnPropertyChanged(System.ComponentModel.PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            string[] settingsProperties = [
                nameof(Hours), nameof(Minutes), nameof(Seconds), nameof(Milliseconds),
                nameof(XCoord), nameof(YCoord), nameof(CoordModeIndex),
                nameof(MouseButtonIndex), nameof(ClickTypeIndex), nameof(Hotkey)
            ];

            if (settingsProperties.Contains(e.PropertyName))
            {
                var prop = typeof(MainViewModel).GetProperty(e.PropertyName ?? string.Empty);
                var newValue = prop?.GetValue(this);

                if (newValue != null)
                {
                    _settings.Save(e.PropertyName ?? string.Empty, newValue);
                }
            }
        }

        partial void OnHotkeyChanged(string value)
        {
            RequestRegisterHotkey?.Invoke();
        }

        [RelayCommand]
        private async Task StartClickingAsync()
        {
            if (IsClicking)
            {
                return;
            }

            IsClicking = true;

            _cts = new CancellationTokenSource();

            _ = int.TryParse(Hours, out int h);
            _ = int.TryParse(Minutes, out int min);
            _ = int.TryParse(Seconds, out int s);
            _ = int.TryParse(Milliseconds, out int ms);
            _ = int.TryParse(XCoord, out int targetX);
            _ = int.TryParse(YCoord, out int targetY);

            int interval = (int)new TimeSpan(h, min, s).TotalMilliseconds + ms;

            if (interval < 1)
            {
                interval = 1;
            }

            string button = MouseButtonIndex switch { 1 => "Right", 2 => "Middle", _ => "Left" };
            string type = ClickTypeIndex == 1 ? "Double" : "Single";

            if (CoordModeIndex == 1)
            {
                _mouseService.MoveMouse(targetX, targetY);
            }

            try
            {
                while (!_cts.Token.IsCancellationRequested)
                {
                    int clickX = targetX;
                    int clickY = targetY;

                    if (CoordModeIndex == 0)
                    {
                        var (X, Y) = _mouseService.GetCursorPosition();

                        clickX = X;
                        clickY = Y;
                    }

                    _mouseService.MoveMouse(clickX, clickY);
                    _mouseService.PerformClick(button, type);

                    await Task.Delay(interval, _cts.Token);
                }
            }
            catch (TaskCanceledException) { }
            finally
            {
                IsClicking = false;
            }
        }

        [RelayCommand]
        private void StartPicking()
        {
            RequestHideWindow?.Invoke();

            _pickerService.StartPicking(
                (x, y) =>
                {
                    XCoord = x.ToString();
                    YCoord = y.ToString();

                    _coordPopupService.IsOpen = false;

                    RequestShowWindow?.Invoke();
                },
                (x, y) =>
                {
                    _coordPopupService.IsOpen = true;
                    _coordPopupService.UpdateCoordinates(x, y);
                    _coordPopupService.SetPositionFromScreenPoint(x, y, 16, 16);
                }
            );
        }

        [RelayCommand]
        private void StopClicking() => _cts?.Cancel();

        [RelayCommand]
        public void ToggleClicking()
        {
            if (IsClicking)
            {
                StopClicking();
            }
            else
            {
                _ = StartClickingAsync();
            }
        }

        [RelayCommand]
        private void Teleport()
        {
            if (int.TryParse(XCoord, out int x) && int.TryParse(YCoord, out int y))
            {
                _mouseService.MoveMouse(x, y);
            }
        }
    }
}
