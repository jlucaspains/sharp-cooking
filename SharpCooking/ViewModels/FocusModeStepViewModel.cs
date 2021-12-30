using System;
using SharpCooking.Models;

namespace SharpCooking.ViewModels
{
    public class FocusModeStepViewModel : BindableModel
    {
        public TimeSpan Time { get; set; }
        public TimeSpan OriginalTime { get; set; }
        public string Title { get; set; }
        public string SubTitle { get; set; }
        public bool HasTime { get; set; }
        public bool IsRunning { get; set; }
        public int NotificationId { get; set; }
        public bool CanStartTimer { get { return HasTime && !IsRunning && Time > TimeSpan.Zero; } }
        public bool CanStopTimer { get { return HasTime && IsRunning; } }
        public bool CanRestartTimer { get { return HasTime && (IsRunning || Time != OriginalTime); } }
    }
}
