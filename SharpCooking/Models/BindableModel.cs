using System.ComponentModel;

namespace SharpCooking.Models
{
    public class BindableModel : INotifyPropertyChanged
    {
#pragma warning disable CS0067
        public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore CS0067

    }
}
