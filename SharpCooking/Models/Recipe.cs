using SQLite;
using System.ComponentModel.DataAnnotations;

namespace SharpCooking.Models
{
    public class Recipe : BindableModel
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [Indexed, Required, SQLite.MaxLength(200), System.ComponentModel.DataAnnotations.MaxLength(200)]
        public string Title { get; set; }
        [Indexed]
        public string Categories { get; set; }
        public bool IsFavorite { get; set; }
        public int Rating { get; set; }
        public string Source { get; set; }
        public string MainImagePath { get; set; }
        public string Ingredients { get; set; }
        public string Instructions { get; set; }
        public string Notes { get; set; }
    }

    public class Uom
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [Required, SQLite.MaxLength(200), System.ComponentModel.DataAnnotations.MaxLength(200)]
        public string Name { get; set; }
        public Dimension Type { get; set; }
    }

    public enum Dimension
    {
        Weight,
        Volume,
        Units
    }
}