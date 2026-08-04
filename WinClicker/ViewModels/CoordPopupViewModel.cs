using CommunityToolkit.Mvvm.ComponentModel;

namespace WinClicker.ViewModels
{
    public partial class CoordPopupViewModel : ObservableObject
    {
        [ObservableProperty]
        public partial double X { get; set; } = 0;

        [ObservableProperty]
        public partial double Y { get; set; } = 0;
    }
}
