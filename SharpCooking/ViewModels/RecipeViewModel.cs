using System;
using System.IO;

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
        public void ApplyMainImage(string fileName)
        {
            var documentFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var actualFile = Path.GetFileName(fileName);

            this.MainImagePath = Path.Combine(documentFolder, actualFile);
        }

        public static Recipe ToModel(RecipeViewModel viewModel)
        {
            if (viewModel == null) throw new ArgumentNullException(nameof(viewModel));

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
                MainImagePath = string.IsNullOrEmpty(viewModel.MainImagePath) ? null : Path.GetFileName(viewModel.MainImagePath)
            };
        }

        public static RecipeViewModel FromModel(Recipe model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));

            var result = new RecipeViewModel
            {
                Id = model.Id,
                Title = model.Title,
                Categories = model.Categories,
                IsFavorite = model.IsFavorite,
                Rating = model.Rating,
                Source = model.Source,
                Ingredients = model.Ingredients,
                Instructions = model.Instructions,
                Notes = model.Notes
            };

            if (!string.IsNullOrEmpty(model.MainImagePath))
                result.ApplyMainImage(model.MainImagePath);

            return result;
        }
    }
}