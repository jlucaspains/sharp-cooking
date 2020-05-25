using System.ComponentModel.DataAnnotations;
using System.Collections.ObjectModel;
using Xamarin.Forms;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SharpCooking.Models
{
    public class RecipeViewModel : BindableModel
    {
        public int Id { get; set; }
        [Required, MaxLength(200)]
        public string Title { get; set; }
        public string Categories { get; set; }
        public int? Yield { get; set; }
        public int? ActiveTime { get; set; }
        public int? TotalTime { get; set; }
        public bool IsFavorite { get; set; }
        public int Rating { get; set; }
        public string Source { get; set; }
        public string Ingredients { get; set; }
        public string Instructions { get; set; }
        public StreamImageSource MainImage { get; set; }
        public bool DoesNotHaveMainImage
        {
            get
            {
                return MainImage == null;
            }
        }
        public bool HasMainImage
        {
            get
            {
                return MainImage != null;
            }
        }
        public ObservableCollection<byte[]> Pictures { get; set; }

        public static async Task<Recipe> ToModel(RecipeViewModel viewModel)
        {
            var cancellationToken = new CancellationToken();
            byte[] mainImage = null;

            if (viewModel.MainImage != null)
                mainImage = ReadFully(await viewModel.MainImage.Stream(cancellationToken));

            return new Recipe
            {
                Id = viewModel.Id,
                Title = viewModel.Title,
                Categories = viewModel.Categories,
                Yield = viewModel.Yield,
                ActiveTime = viewModel.ActiveTime,
                TotalTime = viewModel.TotalTime,
                IsFavorite = viewModel.IsFavorite,
                Rating = viewModel.Rating,
                Source = viewModel.Source,
                Ingredients = viewModel.Ingredients,
                Instructions = viewModel.Instructions,
                MainImage = mainImage
            };
        }

        public static Task<RecipeViewModel> FromModel(Recipe model)
        {
            var result = new RecipeViewModel
            {
                Id = model.Id,
                Title = model.Title,
                Categories = model.Categories,
                Yield = model.Yield,
                ActiveTime = model.ActiveTime,
                TotalTime = model.TotalTime,
                IsFavorite = model.IsFavorite,
                Rating = model.Rating,
                Source = model.Source,
                Ingredients = model.Ingredients,
                Instructions = model.Instructions,
                MainImage = Convert(model?.MainImage)
            };

            return Task.FromResult(result);
        }

        static StreamImageSource Convert(byte[] sourceArray)
        {
            if (sourceArray == null) return null;

            var stream = new MemoryStream(sourceArray);

            return (StreamImageSource)ImageSource.FromStream(() => stream);
        }

        static byte[] ReadFully(Stream input)
        {
            if (input == null) return null;

            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }
    }
}