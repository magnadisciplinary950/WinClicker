using Microsoft.UI.Xaml.Controls;
using WinClicker.ViewModels;

namespace WinClicker.Views
{
    public sealed partial class CoordPopupView : UserControl
    {
        public CoordPopupViewModel ViewModel { get; } = new CoordPopupViewModel();

        public CoordPopupView() => this.InitializeComponent();
    }
}
