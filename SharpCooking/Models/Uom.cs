using SQLite;
using System.ComponentModel.DataAnnotations;

namespace SharpCooking.Models
{
    public class Uom
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [Required, SQLite.MaxLength(200), System.ComponentModel.DataAnnotations.MaxLength(200)]
        public string Name { get; set; }
        public Dimension Type { get; set; }
    }
}