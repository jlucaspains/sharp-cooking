using System.Collections.Generic;

namespace SharpCooking.Models
{
    public class RecipeDownloadConfig
    {
        public string Hostname { get; set; }
        public IEnumerable<string> TitleXPath { get; set; }
        public IEnumerable<string> ImageXPath { get; set; }
        public string ImageAttribute { get; set; }
        public IEnumerable<string> IngredientsXPath { get; set; }
        public IEnumerable<string> PreparationXPath { get; set; }
    }
}