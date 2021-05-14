using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Watermark.WebPublisher.Models
{
    public class Vehicle
    {
        [Key]
        public int Id { get; set; }

        [StringLength(100)]
        public string Brand { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Range(1, 100)]
        public int Stock { get; set; }

        [StringLength(100)]
        public string PictureUrl { get; set; }
    }
}
