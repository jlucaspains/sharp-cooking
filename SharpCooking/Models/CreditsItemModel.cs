namespace SharpCooking.Models
{
    public class CreditsItemModel : BindableModel
    {
        public string Title { get; set; }
        public string ShortDescription { get; set; }
        public string License { get; set; }
        public string ProjectLink { get; set; }
    }
}
