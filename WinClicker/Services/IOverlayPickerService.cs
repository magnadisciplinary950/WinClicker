using System;

namespace WinClicker.Services
{
    public interface IOverlayPickerService
    {
        void StartPicking(Action<int, int> onPick, Action<int, int> onMouseMove);
        void StopPicking();
    }
}
