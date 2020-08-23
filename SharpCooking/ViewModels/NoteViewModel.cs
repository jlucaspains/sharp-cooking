using SharpCooking.Models;

namespace SharpCooking.ViewModels
{
    public class NoteViewModel : BindableModel
    {
        public int Number { get; set; }
        public string Content { get; set; }
    }
}
