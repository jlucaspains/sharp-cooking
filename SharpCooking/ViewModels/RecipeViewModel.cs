namespace SharpCooking.Models
{
    public class RecipeViewModel : BindableModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Categories { get; set; }
        public bool IsFavorite { get; set; }
        public int Rating { get; set; }
        public string Source { get; set; }
        public string Ingredients { get; set; }
        public string Instructions { get; set; }
        public string Notes { get; set; }
        public string MainImagePath { get; set; }
        public bool DoesNotHaveMainImage
        {
            get
            {
                return string.IsNullOrEmpty(MainImagePath);
            }
        }
        public bool HasMainImage
        {
            get
            {
                return !string.IsNullOrEmpty(MainImagePath);
            }
        }

        public static Recipe ToModel(RecipeViewModel viewModel)
        {
            return new Recipe
            {
                Id = viewModel.Id,
                Title = viewModel.Title,
                Categories = viewModel.Categories,
                IsFavorite = viewModel.IsFavorite,
                Rating = viewModel.Rating,
                Source = viewModel.Source,
                Ingredients = viewModel.Ingredients,
                Instructions = viewModel.Instructions,
                Notes = viewModel.Notes,
                MainImagePath = viewModel.MainImagePath
            };
        }

        public static RecipeViewModel FromModel(Recipe model)
        {
            return new RecipeViewModel
            {
                Id = model.Id,
                Title = model.Title,
                Categories = model.Categories,
                IsFavorite = model.IsFavorite,
                Rating = model.Rating,
                Source = model.Source,
                Ingredients = model.Ingredients,
                Instructions = model.Instructions,
                Notes = model.Notes,
                MainImagePath = model.MainImagePath
            };
        }
    }
}