using SQLite;
using System.ComponentModel.DataAnnotations;
using System.Collections.ObjectModel;
using System.Collections.Generic;

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
        public int? Yield { get; set; }
        public int? ActiveTime { get; set; }
        public int? TotalTime { get; set; }
        public bool IsFavorite { get; set; }
        public int Rating { get; set; }
        public string Source { get; set; }
        public byte[] MainImage { get; set; }
        [Ignore]
        public IEnumerable<byte[]> AllImages { get; set; }
        public string Ingredients { get; set; }
        public string Instructions { get; set; }
    }

    //public class RecipeIngredient
    //{
    //    [PrimaryKey, AutoIncrement]
    //    public int Id { get; set; }
    //    public int RecipeId { get; set; }
    //    [Ignore]
    //    public Recipe Recipe { get; set; }
    //    [Indexed, SQLite.MaxLength(200), System.ComponentModel.DataAnnotations.MaxLength(200)]
    //    public string Description { get; set; }
    //    public decimal? Quantity { get; set; }
    //}

    //public class RecipeInstruction
    //{
    //    [PrimaryKey, AutoIncrement]
    //    public int Id { get; set; }
    //    public int RecipeId { get; set; }
    //    [Ignore]
    //    public Recipe Recipe { get; set; }
    //    public int Order { get; set; }
    //    [SQLite.MaxLength(8000), System.ComponentModel.DataAnnotations.MaxLength(8000)]
    //    public string Text { get; set; }
    //}

    public class RecipePictures
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public int RecipeId { get; set; }
        [Ignore]
        public Recipe Recipe { get; set; }
        public bool IsPrimary { get; set; }
        public byte[] Picture { get; set; }
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