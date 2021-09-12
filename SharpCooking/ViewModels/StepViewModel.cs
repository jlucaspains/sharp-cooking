using SharpCooking.Models;

namespace SharpCooking.ViewModels
{
    public class StepViewModel : BindableModel
    {
        public string Time { get; set; }
        public string Title { get; set; }
        public string SubTitle { get; set; }
        public bool IsNotLast { get; set; }
        public bool HasTime { get; set; }
    }
}
