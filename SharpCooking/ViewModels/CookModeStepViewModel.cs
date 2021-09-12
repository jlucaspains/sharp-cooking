using System;
using SharpCooking.Models;

namespace SharpCooking.ViewModels
{
    public class CookModeStepViewModel : BindableModel
    {
        public TimeSpan Time { get; set; }
        public string Title { get; set; }
        public string SubTitle { get; set; }
        public bool HasTime { get; set; }
        public bool IsRunning { get; set; }
        public bool CanStartTimer { get { return HasTime && !IsRunning; } }
        public bool CanStopTimer { get { return HasTime && IsRunning; } }
    }
}
