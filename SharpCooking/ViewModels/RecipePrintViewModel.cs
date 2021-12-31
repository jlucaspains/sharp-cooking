using System;
using System.Collections.Generic;
using System.Linq;
using SharpCooking.Data;
using SharpCooking.Services;

namespace SharpCooking.Models
{
    public class RecipePrintViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int Rating { get; set; } = 3;
        public bool HasRating { get { return Rating > 0; } }
        public string Source { get; set; }
        public IEnumerable<string> Ingredients { get; set; }
        public IEnumerable<string> Instructions { get; set; }
        public string Notes { get; set; }

        public string Base64MainImage { get; private set; }

        public static RecipePrintViewModel FromModel(Recipe model, IFileHelper helper)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            if (helper == null) throw new ArgumentNullException(nameof(helper));

            var result = new RecipePrintViewModel
            {
                Id = model.Id,
                Title = model.Title,
                Rating = model.Rating,
                Source = model.Source,
                Ingredients = Helpers.BreakTextIntoList(model.Ingredients),
                Instructions = Helpers.BreakTextIntoList(model.Instructions),
                Notes = model.Notes
            };

            if(!string.IsNullOrEmpty(model.MainImagePath))
            {
                var fileContents = helper.ReadBytes(model.MainImagePath);

                result.Base64MainImage = Convert.ToBase64String(fileContents);
            }

            return result;
        }
    }
}