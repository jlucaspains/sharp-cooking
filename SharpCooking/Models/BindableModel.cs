using System.ComponentModel;

namespace SharpCooking.Models
{
    public class BindableModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
