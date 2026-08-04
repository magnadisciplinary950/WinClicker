namespace WinClicker.Models
{
    public class ClickSettings
    {
        public int X { get; set; }
        public int Y { get; set; }
        public string MouseButton { get; set; } = "Left";
        public string ClickType { get; set; } = "Single";
        public int IntervalMs { get; set; } = 100;
        public bool UseCurrentPosition { get; set; }
    }
}
